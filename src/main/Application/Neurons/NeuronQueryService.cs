using ei8.Cortex.Diary.Common;
using ei8.Cortex.Graph.Client;
using ei8.Cortex.IdentityAccess.Client.Out;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons
{
    public class NeuronQueryService : INeuronQueryService
    {
        private INeuronGraphQueryClient graphQueryClient;
        private IValidationClient validationClient;
        private ISettingsService settingsService;

        public NeuronQueryService(INeuronGraphQueryClient graphQueryClient, IValidationClient validationClient, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(graphQueryClient, nameof(graphQueryClient));
            AssertionConcern.AssertArgumentNotNull(validationClient, nameof(validationClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.graphQueryClient = graphQueryClient;
            this.validationClient = validationClient;
            this.settingsService = settingsService;            
        }

        public async Task<IEnumerable<Neuron>> GetNeurons(NeuronQuery neuronQuery, Guid subjectId, CancellationToken token = default(CancellationToken))
        {
            var result = await this.graphQueryClient.GetNeurons(
                this.settingsService.CortexGraphOutBaseUrl + "/",                
                neuronQuery.ToExternalType(),
                token
                );

            // TODO: call this.validationClient.ReadNeurons
            // TODO: remove neurons with errors from result set

            return result.Select(cn => cn.ToInternalType());
        }

        public async Task<IEnumerable<Neuron>> GetNeurons(string centralId, NeuronQuery neuronQuery, Guid subjectId, CancellationToken token = default(CancellationToken))
        {
            var result = await this.graphQueryClient.GetNeurons(
                this.settingsService.CortexGraphOutBaseUrl + "/",
                centralId,
                neuronQuery.ToExternalType(),
                token
                );

            // TODO: call this.validationClient.ReadNeurons
            // TODO: remove neurons with errors from result set

            return result.Select(cn => cn.ToInternalType());
        }

        private static Guid? GetNullableStringGuid(string value)
        {
            return (value == null ? (Guid?) null : Guid.Parse(value));
        }

        public async Task<Neuron> GetNeuronById(string id, NeuronQuery neuronQuery, Guid subjectId, CancellationToken token = default(CancellationToken))
        {
            var result = await this.graphQueryClient.GetNeuronById(
                this.settingsService.CortexGraphOutBaseUrl + "/",
                id,
                neuronQuery.ToExternalType(),
                token
                );

            var avr = await this.validationClient.ReadNeurons(
                this.settingsService.IdentityAccessOutBaseUrl + "/",
                new Guid[] { Guid.Parse(id) },
                subjectId,
                token
                );

            // TODO: avr.HasErrors()

            // TODO: remove neurons with errors from result set

            return result.ToInternalType();
        }

        public async Task<IEnumerable<Neuron>> GetNeuronById(string id, string centralId, NeuronQuery neuronQuery, Guid subjectId, CancellationToken token = default(CancellationToken))
        {
            var result = await this.graphQueryClient.GetNeuronById(
                this.settingsService.CortexGraphOutBaseUrl + "/",
                id,
                centralId,
                neuronQuery.ToExternalType(),
                token
                );

            // TODO: call this.validationClient.ReadNeurons
            // TODO: remove neurons with errors from result set

            return result.Select(cn => cn.ToInternalType());
        }
    }
}
