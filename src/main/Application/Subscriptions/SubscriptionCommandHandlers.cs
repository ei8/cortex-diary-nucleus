using CQRSlite.Commands;
using ei8.Cortex.Diary.Nucleus.Application.Subscriptions.Commands;
using ei8.Cortex.Subscriptions.Client;
using neurUL.Common.Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Subscriptions
{
    public class SubscriptionCommandHandlers : ICancellableCommandHandler<AddSubscription>
    {
        private readonly ISubscriptionsClient subscriptionsClient;
        private readonly ISettingsService settingsService;

        public SubscriptionCommandHandlers(ISubscriptionsClient subscriptionsClient,
            ISettingsService settingsService)
        {
            this.subscriptionsClient = subscriptionsClient;
            this.settingsService = settingsService;
        }
        public async Task Handle(AddSubscription message, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));
            AssertionConcern.AssertArgumentNotNull(message.SubscriptionInfo, nameof(message.SubscriptionInfo));

            await this.subscriptionsClient.AddSubscription(settingsService.SubscriptionsInBaseUrl, message.SubscriptionInfo, token);
        }
    }
}
