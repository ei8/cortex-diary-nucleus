using ei8.Cortex.Subscriptions.Common;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Subscriptions
{
    public interface ISubscriptionConfigurationQueryService
    {
        Task<SubscriptionConfiguration> GetServerConfigurationAsync();
    }
}
