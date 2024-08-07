using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
    }
}
