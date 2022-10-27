using CQRSlite.Commands;
using ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands;
using ei8.Cortex.IdentityAccess.Client.In;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Subscriptions.Client.In;
using ei8.Cortex.Subscriptions.Common;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.Out;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons
{
    public class NeuronCommandHandlers : 
        ICancellableCommandHandler<CreateNeuron>,
        ICancellableCommandHandler<ChangeNeuronTag>,
        ICancellableCommandHandler<ChangeNeuronExternalReferenceUrl>,
        ICancellableCommandHandler<DeactivateNeuron>,
        ICancellableCommandHandler<CreateNeuronAccessRequest>
    {
        private readonly INeuronAdapter neuronAdapter;
        private readonly IAuthoredEventStore eventStore;
        private readonly IInMemoryAuthoredEventStore inMemoryEventStore;
        private readonly ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter;
        private readonly ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter;
        private readonly ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;
        private readonly IAccessRequestClient accessRequestClient;
        private readonly INotificationClient notificationClient;
        private readonly ISubscriptionsClient subscriptionsClient;

        public NeuronCommandHandlers(
            IAuthoredEventStore eventStore, 
            IInMemoryAuthoredEventStore inMemoryEventStore,
            INeuronAdapter neuronAdapter,
            ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter,
            ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter,
            ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter, 
            IValidationClient validationClient, 
            ISettingsService settingsService,
            IAccessRequestClient accessRequestClient,
            INotificationClient notificationClient,
            ISubscriptionsClient subscriptionsClient
            )
        {
            AssertionConcern.AssertArgumentNotNull(neuronAdapter, nameof(neuronAdapter));
            AssertionConcern.AssertArgumentNotNull(eventStore, nameof(eventStore));
            AssertionConcern.AssertArgumentNotNull(inMemoryEventStore, nameof(inMemoryEventStore));
            AssertionConcern.AssertArgumentNotNull(tagItemAdapter, nameof(tagItemAdapter));
            AssertionConcern.AssertArgumentNotNull(aggregateItemAdapter, nameof(aggregateItemAdapter));
            AssertionConcern.AssertArgumentNotNull(externalReferenceAdapter, nameof(externalReferenceAdapter));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));
            AssertionConcern.AssertArgumentNotNull(accessRequestClient, nameof(accessRequestClient));
            AssertionConcern.AssertArgumentNotNull(notificationClient, nameof(notificationClient));
            AssertionConcern.AssertArgumentNotNull(subscriptionsClient, nameof(subscriptionsClient));

            this.neuronAdapter = neuronAdapter;
            this.eventStore = (IAuthoredEventStore) eventStore;
            this.inMemoryEventStore = (IInMemoryAuthoredEventStore) inMemoryEventStore;
            this.tagItemAdapter = tagItemAdapter;
            this.aggregateItemAdapter = aggregateItemAdapter;
            this.externalReferenceAdapter = externalReferenceAdapter;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
            this.accessRequestClient = accessRequestClient;
            this.notificationClient = notificationClient;
            this.subscriptionsClient = subscriptionsClient;
        }

        public async Task Handle(CreateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            // validate
            var validationResult = await this.validationClient.CreateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                message.Id,
                message.RegionId,
                message.UserId,
                token);

            if (!validationResult.HasErrors)
            {
                //TODO: transfer all of this to Domain.Model, especially parse of Guid for region/aggregate
                var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId);
                await txn.InvokeAdapter(
                    typeof(NeuronCreated).Assembly,
                    async (ev) => await this.neuronAdapter.CreateNeuron(message.Id, validationResult.UserNeuronId)
                    );
                // assign tag value
                await txn.InvokeAdapter(
                    typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly,
                    async (ev) => await this.tagItemAdapter.ChangeTag(
                        message.Id,
                        message.Tag,
                        validationResult.UserNeuronId,
                        ev
                    ));
                if (message.RegionId.HasValue)
                {
                    // assign region value to id
                    await txn.InvokeAdapter(
                        typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly,
                        async (ev) => await this.aggregateItemAdapter.ChangeAggregate(
                            message.Id,
                            message.RegionId.ToString(),
                            validationResult.UserNeuronId,
                            ev
                        ));
                }
                if (!string.IsNullOrWhiteSpace(message.ExternalReferenceUrl))
                {
                    await txn.InvokeAdapter(
                        typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly,
                        async (ev) => await this.externalReferenceAdapter.ChangeUrl(
                            message.Id,
                            message.ExternalReferenceUrl,
                            validationResult.UserNeuronId,
                            ev
                        ));
                }
                await txn.Commit();
            }
        }

        public async Task Handle(ChangeNeuronTag message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            // validate
            var validationResult = await this.validationClient.UpdateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                message.Id,
                message.UserId,
                token);

            if (!validationResult.HasErrors)
            {
                var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId, message.ExpectedVersion);
                await txn.InvokeAdapter(
                    typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly,
                    async (ev) => await this.tagItemAdapter.ChangeTag(
                        message.Id,
                        message.NewTag,
                        validationResult.UserNeuronId,
                        ev
                    ));
                await txn.Commit();
            }
        }

        public async Task Handle(ChangeNeuronExternalReferenceUrl message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            // validate
            var validationResult = await this.validationClient.UpdateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                message.Id,
                message.UserId,
                token);

            if (!validationResult.HasErrors)
            {
                var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId, message.ExpectedVersion);
                await txn.InvokeAdapter(
                    typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly,
                    async (ev) => await this.externalReferenceAdapter.ChangeUrl(
                        message.Id,
                        message.NewExternalReferenceUrl,
                        validationResult.UserNeuronId,
                        ev
                    ));

                await txn.Commit();
            }
        }

        public async Task Handle(DeactivateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            // validate
            var validationResult = await this.validationClient.UpdateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                message.Id,
                message.UserId,
                token);

            if (!validationResult.HasErrors)
            {
                var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId, message.ExpectedVersion);
                await txn.InvokeAdapter(
                    typeof(NeuronCreated).Assembly,
                    async (ev) => await this.neuronAdapter.DeactivateNeuron(
                        message.Id,
                        validationResult.UserNeuronId,
                        ev
                    ));

                await txn.Commit();
            }
        }

        public async Task Handle(CreateNeuronAccessRequest message, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            await this.accessRequestClient.CreateAccessRequestAsync(this.settingsService.IdentityAccessInBaseUrl, message.NeuronId, message.UserNeuronId.ToString(), token);

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