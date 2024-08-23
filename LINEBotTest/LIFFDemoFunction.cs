using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using static Azure.Core.HttpHeader;
using ihoseco;
using System;
using TimeZoneConverter;

namespace LINEBotTest
{
    public class LIFFDemoFunction
    {
        private readonly ILogger<LIFFDemoFunction> _logger;

        private static string EndpointUri;
        private static string PrimaryKey;
        private static CosmosClient cosmosClient = null;
        private static string databaseId = string.Empty;
        private static string containerId = string.Empty;
        private static Microsoft.Azure.Cosmos.Container container = null;


        static LIFFDemoFunction()
        {
            EndpointUri = GetEnvironmentVariable("AZURE_COSMOSDB_KEY_URI");
            PrimaryKey = GetEnvironmentVariable("AZURE_COSMOSDB_PRIMARY_KEY");
            //コスモスクライアント用オブジェクトを生成する。
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Direct
            });
            //CosmosDBデータベースID文字列
            databaseId = GetEnvironmentVariable("AZURE_COSMOSDB_DATABASE_NAME", "ihosecoDatabase");
            //CosmosDBデータベースコンテナ文字列
            containerId = GetEnvironmentVariable("AZURE_COSMOSDB_CONTAINER_NAME", "ihosecoContainer");
            container = cosmosClient.GetContainer(databaseId, containerId);
        }
        public LIFFDemoFunction(ILogger<LIFFDemoFunction> logger)
        {
            _logger = logger;
        }
        public List<string> SplitCsvString(string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return new List<string>();
            }

            // カンマで分割してList<string>に変換
            List<string> result = new List<string>(csv.Split(','));

            // 各要素の前後の空白をトリム
            for (int i = 0; i < result.Count; i++)
            {
                result[i] = result[i].Trim();
            }

            return result;
        }
        public int GetVoltStatus(int lastvolt)
        {
            int voltstatus = -1;
            if (lastvolt >= 29 && lastvolt <= 31)
            {
                voltstatus = 0;
            }
            else if (lastvolt >= 26 && lastvolt <= 28)
            {
                voltstatus = 1;
            }
            else if (lastvolt >= 22 && lastvolt <= 25)
            {
                voltstatus = 2;
            }
            else
            {
                voltstatus = 3;
            }
            return voltstatus;
        }
        public int IsWithin24Hours(DateTime acceldateTime, DateTime dateTimenow)
        {
            TimeSpan timeDifference = dateTimenow - acceldateTime;
            return timeDifference.TotalHours <= 24 ? 1 : 0;
        }
        public int GetHeartbeatStatus(DateTime heartbeattime, DateTime dateTimenow)
        {
            DateTime lastheartbeatdatetime = heartbeattime;

            if (dateTimenow > lastheartbeatdatetime)
            {
                TimeSpan timeDifference = dateTimenow - lastheartbeatdatetime;
                double hoursDifference = timeDifference.TotalHours;

                if (hoursDifference < 36)
                {
                    return 0;
                }
                if (hoursDifference < 60)
                {
                    return 1;
                }
                if (hoursDifference < 84)
                {
                    return 2;
                }
                return -1;
            }
            return 0;
        }
        [Function("LIFFDemoFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("************************** LIFFDemoFunction. **********************");

            string responseMessage = string.Empty;
            //string csv = req.Query["sensor"];
            string csv = "26F5A06,26F5A07,26F5A10,26F5A16";
            List<string> sensorIds = SplitCsvString(csv);

            DateTime dateTimenow = Utility.GetJapanseseNow();

            var query = $"SELECT c.id, c.sensorid, c.isAccelNotifyOff, c.lastaccelerometerdatetime, c.lastpatroldatetime, c.lastheartbeatdatetime, c.sensorname, c.lastvolt, c.expiredate, c.pictureurl, c.address, c.sensortype, c.longitude, c.latitude, c.picturefilename, c.picturethumbnailfilename  FROM c WHERE c.id IN ({string.Join(", ", sensorIds.Select(id => $"'{id}'"))})";
            query += " ORDER BY c.lastaccelerometerdatetime DESC";

            try
            {

                List<SensorMasterEx> items = await GetItemListFromContainerAsync<SensorMasterEx>(query);

                List<EventListItemEx> demoEventList = new List<EventListItemEx>();
                int no = 0;
                foreach (SensorMasterEx sm in items)
                {
                    DateTime lastaccelerometerdatetime = sm.lastaccelerometerdatetime;
                    string sensorid = sm.sensorid;
                    DateTime lastpatroldatetime = sm.lastpatroldatetime;
                    DateTime lastheartbeatdatetime = sm.lastheartbeatdatetime;
                    DateTime expiredate = sm.expiredate;
                    string sensorname = sm.sensorname;
                    int lastvolt = sm.lastvolt;

                    no++;
                    EventListItemEx sl = new EventListItemEx();
                    sl.no = no;
                    sl.sensorid = sensorid;
                    sl.voltstatus = GetVoltStatus(sm.lastvolt);
                    sl.lastaccelstatus = IsWithin24Hours(sm.lastaccelerometerdatetime, dateTimenow);
                    sl.lastheartbeatstatus = GetHeartbeatStatus(sm.lastheartbeatdatetime, dateTimenow);
                    sl.caption = sensorname;
                    sl.isAccelNotifyOff = sm.isAccelNotifyOff;
                    demoEventList.Add(sl);
                }

                // ダミーデータの作成
                //for (int i = 1; i <= 12; i++)
                //{
                //    demoEventList.Add(new EventListItemEx
                //    {
                //        no = i,
                //        sensorid = $"Sensor_{i}",
                //        // voltstatusは0～3の値をランダムに設定
                //        voltstatus = i % 4, // 0, 1, 2, 3のいずれか
                //        // lastaccelstatusは0または1の値をランダムに設定
                //        lastaccelstatus = i % 2, // 0, 1のいずれか
                //        // lastheartbeatstatusは0～2の値をランダムに設定
                //        lastheartbeatstatus = i % 3, // 0, 1, 2のいずれか
                //        caption = $"Sensor Name {i}",
                //        isAccelNotifyOff = false
                //    });
                //}

                // レスポンスをJSONにシリアライズして返す
                responseMessage = JsonConvert.SerializeObject(demoEventList);
                return new OkObjectResult(responseMessage);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.Message);
            }
            return new BadRequestObjectResult(responseMessage);
        }
        public static string GetEnvironmentVariable(string key, string defaultvalue = null)
        {
            string? env_value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(env_value))
            {
                return defaultvalue;
            }
            return env_value;
        }
        public async Task<T> GetItemFromContainerAsync<T>(string sqlQueryText)
        {
            T result = default(T);
            try
            {
                //log.LogInformation("GetItemFromContainerAsync query: " + sqlQueryText);

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<T> queryResultSetIterator = container.GetItemQueryIterator<T>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    if (currentResultSet.Count > 0)
                    {
                        result = currentResultSet.Resource.First();
                        break;
                    }
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError("GetItemFromContainerAsync exception " + ex.ToString());
            }
            return result;
        }
        public async Task<List<T>> GetItemListFromContainerAsync<T>(string sqlQueryText)
        {
            List<T> items = new List<T>();
            try
            {
                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<T> queryResultSetIterator = container.GetItemQueryIterator<T>(queryDefinition);


                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (var item in currentResultSet)
                    {
                        items.Add(item);
                    }
                }
                return items;
            }
            catch (CosmosException ex)
            {
                _logger.LogError("GetItemListFromContainerAsync exception " + ex.ToString());
            }
            return items;
        }
    }
    public class EventListItemEx
    {
        public int no { get; set; }
        public string sensorid { get; set; }
        public int voltstatus { get; set; } // 0, 1, 2, 3のいずれか
        public int lastaccelstatus { get; set; } // 0, 1のいずれか
        public int lastheartbeatstatus { get; set; } // 0, 1, 2のいずれか
        public string caption { get; set; }
        public bool isAccelNotifyOff { get; set; }
    }
}
