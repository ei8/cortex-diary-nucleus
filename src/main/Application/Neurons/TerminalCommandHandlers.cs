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
        private readonly ITransaction transaction;
        private readonly ITerminalAdapter terminalAdapter;
        private readonly ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter;
        private readonly IValidationClient validationClient;
        private readonly INeuronGraphQueryClient neuronGraphQueryClient;
        private readonly ISettingsService settingsService;

        public TerminalCommandHandlers(
            ITransaction transaction,
            ITerminalAdapter terminalAdapter,
            ei8.Data.ExternalReference.Port.Adapter.In.InProcess.IItemAdapter externalReferenceAdapter, 
            IValidationClient validationClient, 
            INeuronGraphQueryClient neuronGraphQueryClient, 
            ISettingsService settingsService
            )
        {
            AssertionConcern.AssertArgumentNotNull(transaction, nameof(transaction));
            AssertionConcern.AssertArgumentNotNull(terminalAdapter, nameof(terminalAdapter));
            AssertionConcern.AssertArgumentNotNull(externalReferenceAdapter, nameof(externalReferenceAdapter));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(neuronGraphQueryClient, nameof(neuronGraphQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.transaction = transaction;
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
                await this.transaction.BeginAsync(message.Id, validationResult.UserNeuronId);

                var expectedVersion = await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(TerminalCreated).Assembly.GetEventTypes(),
                    async (ev) => await this.terminalAdapter.CreateTerminal(
                        message.Id,
                        message.PresynapticNeuronId,
                        message.PostsynapticNeuronId,
                        message.Effect,
                        message.Strength,
                        validationResult.UserNeuronId
                    ));

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
                await this.transaction.BeginAsync(message.Id, validationResult.UserNeuronId);
                await this.transaction.InvokeAdapterAsync(
                    message.Id,
                    typeof(TerminalDeactivated).Assembly.GetEventTypes(),
                    async (ev) => await this.terminalAdapter.DeactivateTerminal(
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
