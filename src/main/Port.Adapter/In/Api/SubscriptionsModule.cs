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
                                UserId = bodyAsObject.userId,
                                AvatarUrl = bodyAsObject.avatarUrl,
                                Name = bodyAsObject.name,
                                PushEndpoint = bodyAsObject.pushEndpoint,
                                PushAuth = bodyAsObject.pushAuth,
                                PushP256DH = bodyAsObject.pushP256dh,
                            };

                            await commandSender.Send(new AddSubscription(subscriptionInfo, expectedVersion));
                        },
                        NeuronModule.ConcurrencyExceptionSetter,
                        new string[0],
                        "userId",
                        "avatarUrl",
                        "name",
                        "pushAuth",
                        "pushP256dh",
                        "pushEndpoint"
                        );
            });
        }
    }
}
