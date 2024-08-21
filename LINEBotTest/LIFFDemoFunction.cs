using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;

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

        [Function("LIFFDemoFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("************************** LIFFDemoFunction. **********************");

            List<EventListItemEx> demoEventList = new List<EventListItemEx>();

            // ダミーデータの作成
            for (int i = 1; i <= 12; i++)
            {
                demoEventList.Add(new EventListItemEx
                {
                    no = i,
                    sensorid = $"Sensor_{i}",
                    // voltstatusは0～3の値をランダムに設定
                    voltstatus = i % 4, // 0, 1, 2, 3のいずれか
                    // lastaccelstatusは0または1の値をランダムに設定
                    lastaccelstatus = i % 2, // 0, 1のいずれか
                    // lastheartbeatstatusは0～2の値をランダムに設定
                    lastheartbeatstatus = i % 3, // 0, 1, 2のいずれか
                    caption = $"Sensor Name {i}",
                    isAccelNotifyOff = false
                });
            }

            // レスポンスをJSONにシリアライズして返す
            string responseMessage = JsonConvert.SerializeObject(demoEventList);
            return new OkObjectResult(responseMessage);
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
