using CQRSlite.Commands;
using ei8.Cortex.Diary.Nucleus.Application.Subscriptions.Commands;
using ei8.Cortex.Subscriptions.Client.In;
using ei8.Cortex.Subscriptions.Common;
using ei8.Cortex.Subscriptions.Common.Receivers;
using neurUL.Common.Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Subscriptions
{
    public class BrowserSubscriptionCommandHandlers : SubscriptionCommandHandlers<BrowserReceiverInfo>, ICancellableCommandHandler<AddSubscription<BrowserReceiverInfo>>
    {
        public BrowserSubscriptionCommandHandlers
            (ISubscriptionsClient<BrowserReceiverInfo> subscriptionsClient, ISettingsService settingsService) : base(subscriptionsClient, settingsService)
        {
        }
    }

    public abstract class SubscriptionCommandHandlers<T>
        where T : IReceiverInfo
    {
        private readonly ISubscriptionsClient<T> subscriptionsClient;
        private readonly ISettingsService settingsService;

        public SubscriptionCommandHandlers(ISubscriptionsClient<T> subscriptionsClient,
            ISettingsService settingsService)
        {
            this.subscriptionsClient = subscriptionsClient;
            this.settingsService = settingsService;
        }

        public async Task Handle(AddSubscription<T> message, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));
            AssertionConcern.AssertArgumentNotNull(message.ReceiverInfo, nameof(message.ReceiverInfo));
            AssertionConcern.AssertArgumentNotNull(message.SubscriptionInfo, nameof(message.SubscriptionInfo));

            var request = this.BuildRequestObject(message.SubscriptionInfo, message.ReceiverInfo);

            await this.subscriptionsClient.AddSubscription(settingsService.SubscriptionsInBaseUrl, request, token);
        }

        private IAddSubscriptionReceiverRequest<T> BuildRequestObject(SubscriptionInfo subscriptionInfo, IReceiverInfo receiverInfo)
        {
            switch (receiverInfo)
            {
                case BrowserReceiverInfo br:
                    return (IAddSubscriptionReceiverRequest<T>)new AddSubscriptionWebReceiverRequest()
                    {
                        SubscriptionInfo = subscriptionInfo,
                        ReceiverInfo = br
                    };

                default:
                    throw new NotSupportedException($"Unsupported receiver request type: {receiverInfo.GetType().Name}");
            }
        }
    }
}
