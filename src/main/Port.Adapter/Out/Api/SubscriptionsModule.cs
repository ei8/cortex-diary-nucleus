using ei8.Cortex.Diary.Nucleus.Application.Subscriptions;
using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.Out.Api
{
    public class SubscriptionsModule : NancyModule
    {
        public SubscriptionsModule(ISubscriptionConfigurationQueryService service) : base("/nuclei/un8y/subscriptions")
        {
            this.Get("/config", async (parameters) =>
            {
                var result = await service.GetServerConfigurationAsync();

                return new TextResponse(JsonConvert.SerializeObject(result));
            });
        }
    }
}
