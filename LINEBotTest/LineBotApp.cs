using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static Google.Protobuf.WireFormat;

namespace ihoseco
{
    public class LineBotApp : WebhookApplication
    {
        private LineMessagingClient messagingClient { get; }
        private ILogger Log { get; }

        public LineBotApp(LineMessagingClient lineMessagingClient, ILogger log)
        {
            messagingClient = lineMessagingClient;
            Log = log;
        }
        protected override async Task OnPostbackAsync(PostbackEvent ev)
        {
            Log.LogInformation($"OnPostbackAsync SourceType:{ev.Source.Type}, SourceId:{ev.Source.Id}, PostbackData:{ev.Postback.Data}");
        }
        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            Log.LogInformation($"OnMessageAsync SourceType:{ev.Source.Type}, SourceId:{ev.Source.Id}, MessageType:{ev.Message.Type}");
            if (ev.Message.Type == EventMessageType.Text)
            {
                Log.LogInformation($"Text={((TextEventMessage)ev.Message).Text}");
                string linetype = ev.Source.Type.ToString();
                await HandleTextAsync(ev.ReplyToken, ((TextEventMessage)ev.Message).Text, ev.Source.Id, linetype);
            }
        }
        protected override async Task OnFollowAsync(FollowEvent ev)
        {
            Log.LogInformation($"OnFollowAsync SourceType:{ev.Source.Type}, SourceId:{ev.Source.Id}");
        }

        protected override async Task OnUnfollowAsync(UnfollowEvent ev)
        {
            Log.LogInformation($"OnUnfollowAsync SourceType:{ev.Source.Type}, SourceId:{ev.Source.Id}");
        }

        protected override async Task OnJoinAsync(JoinEvent ev)
        {
            Log.LogInformation($"OnJoinAsync SourceType:{ev.Source.Type}, SourceId:{ev.Source.Id}");
        }

        protected override async Task OnLeaveAsync(LeaveEvent ev)
        {
            Log.LogInformation($"OnLeaveAsync SourceType:{ev.Source.Type}, SourceId:{ev.Source.Id}");
        }
        private async Task HandleTextAsync(string replyToken, string userMessage, string sourceid, string linetype)
        {
            var replyMessage = new TextMessage($"You said: {userMessage}");
            var quickReply = new QuickReply();
            if (userMessage == "こんにちは")
            {
                replyMessage.Text = "本当に暑いですね";
            }
            else if (userMessage == "1" || userMessage == "１")
            {
                quickReply.Items.Add(new QuickReplyButtonObject(
                    new PostbackTemplateAction("中止", "deviceid=dummy&action=cancel&id=000", "中止", true)));
                //センサー登録
                string liffsensoronurl = $"command=sensoronlist&lineid={sourceid}&linetype={linetype}";
                quickReply.Items.Add(new QuickReplyButtonObject(
                    new PostbackTemplateAction("センサー登録", liffsensoronurl, "センサー登録", true)));
                //センサー解除
                string liffoffsensor = $"command=sensorofflist&lineid={sourceid}&linetype={linetype}";
                quickReply.Items.Add(new QuickReplyButtonObject(
                    new PostbackTemplateAction("センサー解除", liffoffsensor, "センサー解除", true)));
                //窓飛HP
                quickReply.Items.Add(new QuickReplyButtonObject(
                                new UriTemplateAction("窓飛HP", "https://www.sohi.co.jp/index.html"), null));

                ISendMessage replySendMessage = new TextMessage("センサーの登録、解除", quickReply);
                await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replySendMessage });
                return;
            }
            await messagingClient.ReplyMessageAsync(replyToken, new List<ISendMessage> { replyMessage });
        }
    }
}
