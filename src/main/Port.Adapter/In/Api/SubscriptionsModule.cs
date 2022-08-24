using CQRSlite.Commands;
using ei8.Cortex.Diary.Nucleus.Application.Subscriptions.Commands;
using ei8.Cortex.Subscriptions.Common;
using ei8.Cortex.Subscriptions.Common.Receivers;
using Nancy;
using neurUL.Common.Api;
using System;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.In.Api
{
    public class SubscriptionsModule : NancyModule
    {
        public SubscriptionsModule(ICommandSender commandSender) : base("/nuclei/d23/subscriptions")
        {
            this.Post("/receivers/{receiverType}", async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        false,
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            var subscriptionInfo = new SubscriptionInfo()
                            {
                                UserId = Guid.Parse(bodyAsObject.UserId.ToString()),
                                AvatarUrl = bodyAsObject.SubscriptionInfo.AvatarUrl,
                            };

                            var receiverInfo = this.DeserializeReceiverRequest(bodyAsObject.ReceiverInfo, parameters.receiverType);
                            await commandSender.Send(new AddSubscription<IReceiverInfo>(subscriptionInfo, receiverInfo, expectedVersion));
                        },
                        NeuronModule.ConcurrencyExceptionSetter,
                        new string[0],
                        "SubscriptionInfo",
                        "ReceiverInfo",
                        "UserId"
                        );
            });
        }

        private IReceiverInfo DeserializeReceiverRequest(dynamic receiverInfo, string receiverType)
        {
            switch (receiverType)
            {
                case "web":
                    return new BrowserReceiverInfo()
                    {
                        Name = receiverInfo.Name,
                        PushAuth = receiverInfo.PushAuth,
                        PushEndpoint = receiverInfo.PushEndpoint,
                        PushP256DH = receiverInfo.PushP256DH,
                    };

                default:
                    throw new NotSupportedException($"Unsupported receiver type for endpoint {receiverType}");
            }
        }
    }
}
