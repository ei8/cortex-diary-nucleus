using CQRSlite.Commands;
using ei8.Cortex.Diary.Nucleus.Application;
using ei8.Cortex.Diary.Nucleus.Application.Subscriptions.Commands;
using ei8.Cortex.Subscriptions.Common;
using Nancy;
using neurUL.Common.Api;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.In.Api
{
    public class SubscriptionsModule : NancyModule
    {
        public SubscriptionsModule(ICommandSender commandSender,
            ISettingsService settings) : base("/nuclei/d23/subscriptions")
        {
            this.Post("/", async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        false,
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            var subscriptionInfo = new BrowserSubscriptionInfo()
                            {
                                UserId = bodyAsObject.UserId.ToString(),
                                AvatarUrl = bodyAsObject.AvatarUrl,
                                Name = bodyAsObject.Name,
                                PushEndpoint = bodyAsObject.PushEndpoint,
                                PushAuth = bodyAsObject.PushAuth,
                                PushP256DH = bodyAsObject.PushP256DH,
                            };

                            await commandSender.Send(new AddSubscription(subscriptionInfo, expectedVersion));
                        },
                        NeuronModule.ConcurrencyExceptionSetter,
                        new string[0],
                        "AvatarUrl",
                        "Name",
                        "PushAuth",
                        "PushP256DH",
                        "PushEndpoint",
                        "UserId"
                        );
            });
        }
    }
}
