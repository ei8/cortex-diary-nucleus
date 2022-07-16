using CQRSlite.Commands;
using ei8.Cortex.Subscriptions.Common;

namespace ei8.Cortex.Diary.Nucleus.Application.Subscriptions.Commands
{
    public class AddSubscription : ICommand
    {
        public AddSubscription(BrowserSubscriptionInfo subscriptionInfo, int expectedVersion)
        {
            this.SubscriptionInfo = subscriptionInfo;
            this.ExpectedVersion = expectedVersion;
        }

        public BrowserSubscriptionInfo SubscriptionInfo { get; private set; }
        public int ExpectedVersion { get; private set; }
    }
}
