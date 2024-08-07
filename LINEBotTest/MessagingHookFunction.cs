using ihoseco;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LINEBotTest
{
    public class MessagingHookFunction
    {
        private readonly ILogger<MessagingHookFunction> log;
        static LineMessagingClient lineMessagingClient;
        static MessagingHookFunction()
        {
            lineMessagingClient = new LineMessagingClient(GetEnvironmentVariable("LINEAPI_CHANNELACCESSTOKEN"));
        }

        public MessagingHookFunction(ILogger<MessagingHookFunction> logger)
        {
            log = logger;
        }

        [Function("MessagingHookFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            log.LogInformation("*********************** MessagingHookFunction. start **********************");

            string token = GetEnvironmentVariable("LINEAPI_CHANNELACCESSTOKEN");
            string secret = GetEnvironmentVariable("LINEAPI_CHANNELSECRET");
            log.LogInformation($"token={token} secret={secret}");

            //以下のコメントアウトは拡張メソッドを使って、HttpRequestをHttpRequestMessageに変換してからGetWebhookEventsAsyncを呼び出す方法
            var reqmsg = await req.ToHttpRequestMessageAsync();
            log.LogInformation($"End HttpRequest to HttpRequestMessage");

            var events = await reqmsg.GetWebhookEventsAsync(GetEnvironmentVariable("LINEAPI_CHANNELSECRET"));

            log.LogInformation($"End reqmsg.GetWebhookEventsAsync");

            var app = new LineBotApp(lineMessagingClient, log);

            await app.RunAsync(events);

            log.LogInformation("*********************** MessagingHookFunction. end **********************");
            return new StatusCodeResult((int)HttpStatusCode.OK);
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
    }
    public static class HttpRequestExtensions
    {
        public static async Task<HttpRequestMessage> ToHttpRequestMessageAsync(this HttpRequest request)
        {
            var requestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(request.Method),
                RequestUri = new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}")
            };

            // ヘッダーのコピー
            foreach (var header in request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            // ボディのコピー
            if (request.ContentLength > 0 || request.Headers.ContainsKey("Transfer-Encoding"))
            {
                request.EnableBuffering();
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;

                // ストリームの位置をリセット
                request.Body.Position = 0;

                // Contentヘッダーのコピー
                foreach (var header in request.Headers)
                {
                    if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                    {
                        requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }
            }

            return requestMessage;
        }
    }
}
