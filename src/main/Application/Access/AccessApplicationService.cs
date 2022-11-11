using ei8.Cortex.IdentityAccess.Client.In;
using ei8.Cortex.Subscriptions.Client.In;
using ei8.Cortex.Subscriptions.Common;
using ei8.EventSourcing.Client.Out;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Access
{
    public class AccessApplicationService : IAccessApplicationService
    {
        private readonly IAccessRequestClient accessRequestClient;
        private readonly ISettingsService settingsService;
        private readonly ISubscriptionsClient subscriptionsClient;
        private readonly INotificationClient notificationClient;

        public AccessApplicationService(IAccessRequestClient accessRequestClient,
            ISettingsService settingsService,
            ISubscriptionsClient subscriptionsClient,
            INotificationClient notificationClient)
        {
            this.accessRequestClient = accessRequestClient;
            this.settingsService = settingsService;
            this.subscriptionsClient = subscriptionsClient;
            this.notificationClient = notificationClient;
        }

        public async Task CreateNeuronAccessRequest(Guid neuronId, Guid userNeuronId, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(neuronId, nameof(neuronId));
            AssertionConcern.AssertArgumentNotNull(userNeuronId, nameof(userNeuronId));

            await this.accessRequestClient.CreateAccessRequestAsync(this.settingsService.IdentityAccessInBaseUrl, neuronId, userNeuronId.ToString(), token);

            var ownerUserId = await GetOwnerUserNeuronIdAsync(token);

            await this.subscriptionsClient.SendNotificationToUser(this.settingsService.SubscriptionsInBaseUrl, ownerUserId, new NotificationPayloadRequest()
            {
                TemplateType = NotificationTemplate.NeuronAccessRequested,
                TemplateValues = new Dictionary<string, object>()
            }, token);
        }

        private async Task<string> GetOwnerUserNeuronIdAsync(CancellationToken token = default)
        {
            var ownerQueryResult = await this.notificationClient.GetNotificationLog(this.settingsService.EventSourcingOutBaseUrl + "/", "1,20", token);
            var ownerUserId = ownerQueryResult.NotificationList.FirstOrDefault(nl => nl.Id == nl.AuthorId).AuthorId;

            return ownerUserId;
        }
    }
}
