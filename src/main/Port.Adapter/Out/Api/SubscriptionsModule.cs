using ei8.Cortex.Diary.Nucleus.Application;
using ei8.Cortex.Subscriptions.Client.Out;
using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.Out.Api
{
    public class SubscriptionsModule : NancyModule
    {
        public SubscriptionsModule(ISubscriptionsConfigurationClient subscriptionsConfigurationClient,
            ISettingsService settings) : base("/nuclei/d23/subscriptions")
        {
            this.Get("/config", async (parameters) =>
            {
                var result = await subscriptionsConfigurationClient.GetServerConfigurationAsync(settings.SubscriptionsOutBaseUrl);

                return new TextResponse(JsonConvert.SerializeObject(result));
            });
        }
    }
}
