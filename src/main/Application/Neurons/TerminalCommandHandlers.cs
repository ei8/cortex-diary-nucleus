using CQRSlite.Commands;
using CQRSlite.Events;
using ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands;
using ei8.Cortex.Graph.Client;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.EventSourcing.Client;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons
{
    public class TerminalCommandHandlers :
        ICancellableCommandHandler<CreateTerminal>,
        ICancellableCommandHandler<DeactivateTerminal>
    {
        private readonly IAuthoredEventStore eventStore;
        private readonly IInMemoryAuthoredEventStore inMemoryEventStore;
        private readonly ITerminalAdapter terminalAdapter;
        private readonly ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter;
        private readonly IValidationClient validationClient;
        private readonly INeuronGraphQueryClient neuronGraphQueryClient;
        private readonly ISettingsService settingsService;

        public TerminalCommandHandlers(
            IAuthoredEventStore eventStore, 
            IInMemoryAuthoredEventStore inMemoryEventStore, 
            ITerminalAdapter terminalAdapter,
            ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter, 
            IValidationClient validationClient, 
            INeuronGraphQueryClient neuronGraphQueryClient, 
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(eventStore, nameof(eventStore));
            AssertionConcern.AssertArgumentNotNull(inMemoryEventStore, nameof(inMemoryEventStore));
            AssertionConcern.AssertArgumentNotNull(terminalAdapter, nameof(terminalAdapter));
            AssertionConcern.AssertArgumentNotNull(externalReferenceAdapter, nameof(externalReferenceAdapter));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(neuronGraphQueryClient, nameof(neuronGraphQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.eventStore = (IAuthoredEventStore)eventStore;
            this.inMemoryEventStore = (IInMemoryAuthoredEventStore)inMemoryEventStore; 
            this.terminalAdapter = terminalAdapter;
            this.externalReferenceAdapter = externalReferenceAdapter;
            this.validationClient = validationClient;
            this.neuronGraphQueryClient = neuronGraphQueryClient;
            this.settingsService = settingsService;
        }

        public async Task Handle(CreateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            // validate
            var validationResult = await this.validationClient.UpdateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                message.PresynapticNeuronId,
                message.UserId,
                token);

            if (!validationResult.HasErrors)
            {
                var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId);

                var otherAggregateEvents = await (new Guid[] { message.PresynapticNeuronId, message.PostsynapticNeuronId }).SelectManyAsync(g => this.eventStore.Get(g, -1));
                await txn.InvokeAdapter(
                    typeof(TerminalCreated).Assembly,
                    async (ev) => await this.terminalAdapter.CreateTerminal(
                        message.Id,
                        message.PresynapticNeuronId,
                        message.PostsynapticNeuronId,
                        message.Effect,
                        message.Strength,
                        validationResult.UserNeuronId
                    ),
                    Transaction.ReplaceUnrecognizedEvents(otherAggregateEvents, typeof(NeuronCreated).Assembly)
                    );

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

        public async Task Handle(DeactivateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var queryResult = await this.neuronGraphQueryClient.GetTerminalById(
                this.settingsService.CortexGraphOutBaseUrl + "/", 
                message.Id.ToString(),
                new Graph.Common.NeuronQuery() { TerminalActiveValues = Graph.Common.ActiveValues.All },
                token
                );

            var terminal = queryResult.Neurons.FirstOrDefault()?.Terminal;
            AssertionConcern.AssertArgumentValid(t => t != null, terminal, "Specified terminal does not exist.", "Id");

            // validate
            var validationResult = await this.validationClient.UpdateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                Guid.Parse(terminal.PresynapticNeuronId),
                message.UserId,
                token);

            if (!validationResult.HasErrors)
            {
                var txn = await Transaction.Begin(this.eventStore, this.inMemoryEventStore, message.Id, validationResult.UserNeuronId, message.ExpectedVersion);
                await txn.InvokeAdapter(
                    typeof(TerminalDeactivated).Assembly,
                    async (ev) => await this.terminalAdapter.DeactivateTerminal(
                        message.Id,
                        validationResult.UserNeuronId,
                        ev
                    ));

                await txn.Commit();
            }
        }
    }
}
