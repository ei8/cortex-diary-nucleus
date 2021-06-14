using CQRSlite.Commands;
using ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands;
using ei8.Cortex.Graph.Client;
using ei8.Cortex.IdentityAccess.Client.Out;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Client.In;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons
{
    public class TerminalCommandHandlers :
        ICancellableCommandHandler<CreateTerminal>,
        ICancellableCommandHandler<DeactivateTerminal>
    {
        private readonly ITerminalClient terminalClient;
        private readonly IValidationClient validationClient;
        private readonly INeuronGraphQueryClient neuronGraphQueryClient;
        private readonly ISettingsService settingsService;

        public TerminalCommandHandlers(ITerminalClient terminalClient, IValidationClient validationClient, INeuronGraphQueryClient neuronGraphQueryClient, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(terminalClient, nameof(terminalClient));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(neuronGraphQueryClient, nameof(neuronGraphQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.terminalClient = terminalClient;
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
                await this.terminalClient.CreateTerminal(
                    this.settingsService.CortexInBaseUrl + "/",
                    message.Id.ToString(),
                    message.PresynapticNeuronId.ToString(),
                    message.PostsynapticNeuronId.ToString(),
                    message.Effect,
                    message.Strength,
                    validationResult.UserNeuronId.ToString(),
                    token
                );
        }

        public async Task Handle(DeactivateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var terminal = await this.neuronGraphQueryClient.GetTerminalById(
                this.settingsService.CortexGraphOutBaseUrl + "/", 
                message.Id.ToString(),
                new Graph.Common.NeuronQuery() { TerminalActiveValues = Graph.Common.ActiveValues.All },
                token
                );

            // validate
            var validationResult = await this.validationClient.UpdateNeuron(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                Guid.Parse(terminal.PresynapticNeuronId),
                message.UserId,
                token);

            if (!validationResult.HasErrors)
                await this.terminalClient.DeactivateTerminal(
                    this.settingsService.CortexInBaseUrl + "/",
                    message.Id.ToString(),
                    message.ExpectedVersion,
                    validationResult.UserNeuronId.ToString(),
                    token
                );
        }
    }
}
