using ei8.Cortex.Diary.Common;
using ei8.Cortex.Graph.Client;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons
{
    public class NeuronQueryService : INeuronQueryService
    {
        private INeuronGraphQueryClient graphQueryClient;
        private ISettingsService settingsService;

        public NeuronQueryService(INeuronGraphQueryClient graphQueryClient, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(graphQueryClient, nameof(graphQueryClient));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.graphQueryClient = graphQueryClient;
            this.settingsService = settingsService;            
        }

        public async Task<IEnumerable<Neuron>> GetNeurons(string centralId = default(string), RelativeType type = RelativeType.NotSet, NeuronQuery neuronQuery = null, 
            int? limit = 1000, CancellationToken token = default(CancellationToken))
        {
            var result = await this.graphQueryClient.GetNeurons(
                this.settingsService.CortexGraphOutBaseUrl + "/",
                centralId,
                type.ToExternalType(),
                neuronQuery.ToExternalType(),
                limit,
                token
                );
            return result.ToInternalType();
        }

        private static Guid? GetNullableStringGuid(string value)
        {
            return (value == null ? (Guid?) null : Guid.Parse(value));
        }

        public async Task<IEnumerable<Neuron>> GetNeuronById(string id, string centralId = default(string), RelativeType type = RelativeType.NotSet, CancellationToken token = default(CancellationToken))
        {
            var result = await this.graphQueryClient.GetNeuronById(
                this.settingsService.CortexGraphOutBaseUrl + "/",
                id,
                centralId,
                type.ToExternalType(),
                token
                );

            return result.ToInternalType();
        }
    }
}
