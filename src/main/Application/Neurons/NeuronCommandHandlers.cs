using CQRSlite.Commands;
using ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons
{
    public class NeuronCommandHandlers : 
        ICancellableCommandHandler<CreateNeuron>,
        ICancellableCommandHandler<ChangeNeuronTag>,
        ICancellableCommandHandler<ChangeNeuronExternalReferenceUrl>,
        ICancellableCommandHandler<DeactivateNeuron>,
        ICancellableCommandHandler<ChangeNeuronRegionId>
    {
        private readonly ITransaction transaction;
        private readonly INeuronAdapter neuronAdapter;
        private readonly ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter;
        private readonly ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter;
        private readonly ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter;
        private readonly IValidationClient validationClient;
        private readonly ISettingsService settingsService;

        public NeuronCommandHandlers(
            ITransaction transaction,
            INeuronAdapter neuronAdapter,
            ei8.Data.Tag.Port.Adapter.In.InProcess.IItemAdapter tagItemAdapter,
            ei8.Data.Aggregate.Port.Adapter.In.InProcess.IItemAdapter aggregateItemAdapter,
            ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter, 
            IValidationClient validationClient, 
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(neuronAdapter, nameof(neuronAdapter));
            AssertionConcern.AssertArgumentNotNull(tagItemAdapter, nameof(tagItemAdapter));
            AssertionConcern.AssertArgumentNotNull(aggregateItemAdapter, nameof(aggregateItemAdapter));
            AssertionConcern.AssertArgumentNotNull(externalReferenceAdapter, nameof(externalReferenceAdapter));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.transaction = transaction;
            this.neuronAdapter = neuronAdapter;
            this.tagItemAdapter = tagItemAdapter;
            this.aggregateItemAdapter = aggregateItemAdapter;
            this.externalReferenceAdapter = externalReferenceAdapter;
            this.validationClient = validationClient;
            this.settingsService = settingsService;
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
                //TODO: transfer all of this to domain.model.IRepository, especially parse of Guid for region/aggregate
                await this.transaction.BeginAsync(validationResult.UserNeuronId);
                int expectedVersion = await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(NeuronCreated).Assembly.GetEventTypes(),
                    async (ev) => await this.neuronAdapter.CreateNeuron(message.Id, validationResult.UserNeuronId)
                    );
                // assign tag value
                expectedVersion = await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly.GetEventTypes(),
                    async (ev) => await this.tagItemAdapter.ChangeTag(
                        message.Id,
                        message.Tag,
                        validationResult.UserNeuronId,
                        ev
                    ),
                    expectedVersion
                    );
                if (message.RegionId.HasValue)
                {
                    // assign region value to id
                    expectedVersion = await this.transaction.InvokeAdapterAsync(
                        message.Id,
                        typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly.GetEventTypes(),
                        async (ev) => await this.aggregateItemAdapter.ChangeAggregate(
                            message.Id,
                            message.RegionId.ToString(),
                            validationResult.UserNeuronId,
                            ev
                        ),
                        expectedVersion
                        );
                }
                if (!string.IsNullOrWhiteSpace(message.ExternalReferenceUrl))
                {
                    expectedVersion = await this.transaction.InvokeAdapterAsync(
                        message.Id,
                        typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly.GetEventTypes(),
                        async (ev) => await this.externalReferenceAdapter.ChangeUrl(
                            message.Id,
                            message.ExternalReferenceUrl,
                            validationResult.UserNeuronId,
                            ev
                        ),
                        expectedVersion
                        );
                }
                await this.transaction.CommitAsync();
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
                await this.transaction.BeginAsync(validationResult.UserNeuronId);
                await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(ei8.Data.Tag.Domain.Model.TagChanged).Assembly.GetEventTypes(),
                    async (ev) => await this.tagItemAdapter.ChangeTag(
                        message.Id,
                        message.NewTag,
                        validationResult.UserNeuronId,
                        ev
                    ),
                    message.ExpectedVersion
                    );
                await this.transaction.CommitAsync();
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
                await this.transaction.BeginAsync(validationResult.UserNeuronId);
                await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(ei8.Data.ExternalReference.Domain.Model.UrlChanged).Assembly.GetEventTypes(),
                    async (ev) => await this.externalReferenceAdapter.ChangeUrl(
                        message.Id,
                        message.NewExternalReferenceUrl,
                        validationResult.UserNeuronId,
                        ev
                    ),
                    message.ExpectedVersion
                    );

                await this.transaction.CommitAsync();
            }
        }

        public async Task Handle(ChangeNeuronRegionId message, CancellationToken token = default(CancellationToken))
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
                await this.transaction.BeginAsync(validationResult.UserNeuronId);
                await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(ei8.Data.Aggregate.Domain.Model.AggregateChanged).Assembly.GetEventTypes(),
                    async (ev) => await this.aggregateItemAdapter.ChangeAggregate(
                        message.Id,
                        message.NewRegionId,
                        validationResult.UserNeuronId,
                        ev
                    ),
                    message.ExpectedVersion
                    );

                await this.transaction.CommitAsync();
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
                await this.transaction.BeginAsync(validationResult.UserNeuronId);
                await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(NeuronCreated).Assembly.GetEventTypes(),
                    async (ev) => await this.neuronAdapter.DeactivateNeuron(
                        message.Id,
                        validationResult.UserNeuronId,
                        ev
                    ),
                    message.ExpectedVersion
                    );

                await this.transaction.CommitAsync();
            }
        }
    }
}

