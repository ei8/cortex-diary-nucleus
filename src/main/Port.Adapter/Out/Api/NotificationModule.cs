using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using ei8.Cortex.Diary.Nucleus.Application.Notification;
using neurUL.Common;
using System.Linq;
using System.Text;
using ei8.Cortex.Diary.Common;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.Out.Api
{
    public class NotificationModule : NancyModule
    {
        public NotificationModule(INotificationApplicationService notificationService) : base("/nuclei/un8y/notifications")
        {
            this.Get("/", async (parameters) => new TextResponse(JsonConvert.SerializeObject(
                await notificationService.GetNotificationLog(string.Empty)
                ))
            );

            this.Get("/{logid}", async (parameters) => new TextResponse(JsonConvert.SerializeObject(
                await notificationService.GetNotificationLog(parameters.logid)
                ))
            );
        }
    }
}