using ihoseco;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
using System.Text;

namespace LINEBotTest
{
    public static class WebhookRequestMessageHelper
    {
        /// <summary>
        /// Verify if the request is valid, then returns LINE Webhook events from the request
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <param name="channelSecret">ChannelSecret</param>
        /// <param name="botUserId">BotUserId</param>
        /// <returns>List of WebhookEvent</returns>
        public static async Task<IEnumerable<WebhookEvent>> GetWebhookEventsAsync(this HttpRequest request, string channelSecret, string? botUserId = null)
        {
            if (request is null) { throw new ArgumentNullException(nameof(request)); }
            if (channelSecret is null) { throw new ArgumentNullException(nameof(channelSecret)); }

            // Read the request body
            string content;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                content = await reader.ReadToEndAsync();
            }

            var xLineSignature = request.Headers["x-line-signature"].FirstOrDefault();
            if (string.IsNullOrEmpty(xLineSignature) || !VerifySignature(channelSecret, xLineSignature, content))
            {
                throw new InvalidSignatureException("Signature validation failed.");
            }

            dynamic json = JsonConvert.DeserializeObject(content);

            if (!string.IsNullOrEmpty(botUserId))
            {
                if (botUserId != (string)json.destination)
                {
                    throw new UserIdMismatchException("Bot user ID does not match.");
                }
            }
            return WebhookEventParser.ParseEvents(json.events);
        }
        /// <summary>
        /// Verify if the request is valid, then returns LINE Webhook events from the request
        /// </summary>
        /// <param name="request">HttpRequestMessage</param>
        /// <param name="channelSecret">ChannelSecret</param>
        /// <param name="botUserId">BotUserId</param>
        /// <returns>List of WebhookEvent</returns>
        public static async Task<IEnumerable<WebhookEvent>> GetWebhookEventsAsync(this HttpRequestMessage request, string channelSecret, string? botUserId = null)
        {
            if (request is null) { throw new ArgumentNullException(nameof(request)); }
            if (channelSecret is null) { throw new ArgumentNullException(nameof(channelSecret)); }

            var content = await request.Content.ReadAsStringAsync();

            var xLineSignature = request.Headers.GetValues("x-line-signature").FirstOrDefault();
            if (string.IsNullOrEmpty(xLineSignature) || !VerifySignature(channelSecret, xLineSignature, content))
            {
                throw new InvalidSignatureException("Signature validation faild.");
            }

            dynamic json = JsonConvert.DeserializeObject(content);

            if (!string.IsNullOrEmpty(botUserId))
            {
                if (botUserId != (string)json.destination)
                {
                    throw new UserIdMismatchException("Bot user ID does not match.");
                }
            }
            return WebhookEventParser.ParseEvents(json.events);
        }

        /// <summary>
        /// The signature in the X-Line-Signature request header must be verified to confirm that the request was sent from the LINE Platform.
        /// Authentication is performed as follows.
        /// 1. With the channel secret as the secret key, your application retrieves the digest value in the request body created using the HMAC-SHA256 algorithm.
        /// 2. The server confirms that the signature in the request header matches the digest value which is Base64 encoded
        /// https://developers.line.biz/en/reference/messaging-api/#signature-validation
        /// https://developers.line.biz/ja/reference/messaging-api/#signature-validation
        /// </summary>
        /// <param name="channelSecret">ChannelSecret</param>
        /// <param name="xLineSignature">X-Line-Signature header</param>
        /// <param name="requestBody">RequestBody</param>
        /// <returns></returns>
        internal static bool VerifySignature(string channelSecret, string xLineSignature, string requestBody)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(channelSecret);
                var body = Encoding.UTF8.GetBytes(requestBody);

                using HMACSHA256 hmac = new(key);
                var hash = hmac.ComputeHash(body, 0, body.Length);
                var xLineBytes = Convert.FromBase64String(xLineSignature);
                return SlowEquals(xLineBytes, hash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Compares two-byte arrays in length-constant time. 
        /// This comparison method is used so that password hashes cannot be extracted from on-line systems using a timing attack and then attacked off-line.
        /// <para> http://bryanavery.co.uk/cryptography-net-avoiding-timing-attack/#comment-85Å@</para>
        /// </summary>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }
    }

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

            var events = await req.GetWebhookEventsAsync(GetEnvironmentVariable("LINEAPI_CHANNELSECRET"));

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
}
