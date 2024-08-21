using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LINEBotTest
{
    public class LIFFDemoFunction
    {
        private readonly ILogger<LIFFDemoFunction> _logger;

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
