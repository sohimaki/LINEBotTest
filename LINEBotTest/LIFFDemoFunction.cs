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

            // �_�~�[�f�[�^�̍쐬
            for (int i = 1; i <= 12; i++)
            {
                demoEventList.Add(new EventListItemEx
                {
                    no = i,
                    sensorid = $"Sensor_{i}",
                    // voltstatus��0�`3�̒l�������_���ɐݒ�
                    voltstatus = i % 4, // 0, 1, 2, 3�̂����ꂩ
                    // lastaccelstatus��0�܂���1�̒l�������_���ɐݒ�
                    lastaccelstatus = i % 2, // 0, 1�̂����ꂩ
                    // lastheartbeatstatus��0�`2�̒l�������_���ɐݒ�
                    lastheartbeatstatus = i % 3, // 0, 1, 2�̂����ꂩ
                    caption = $"Sensor Name {i}",
                    isAccelNotifyOff = false
                });
            }

            // ���X�|���X��JSON�ɃV���A���C�Y���ĕԂ�
            string responseMessage = JsonConvert.SerializeObject(demoEventList);
            return new OkObjectResult(responseMessage);
        }
    }
    public class EventListItemEx
    {
        public int no { get; set; }
        public string sensorid { get; set; }
        public int voltstatus { get; set; } // 0, 1, 2, 3�̂����ꂩ
        public int lastaccelstatus { get; set; } // 0, 1�̂����ꂩ
        public int lastheartbeatstatus { get; set; } // 0, 1, 2�̂����ꂩ
        public string caption { get; set; }
        public bool isAccelNotifyOff { get; set; }
    }
}
