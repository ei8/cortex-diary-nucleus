using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ei8.Cortex.Diary.Common;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons
{
    public interface INeuronQueryService
    {
        Task<IEnumerable<Neuron>> GetNeurons(NeuronQuery neuronQuery, Guid subjectId, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<Neuron>> GetNeurons(string centralId, NeuronQuery neuronQuery, Guid subjectId, CancellationToken token = default(CancellationToken));

        Task<Neuron> GetNeuronById(string id, NeuronQuery neuronQuery, Guid subjectId, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<Neuron>> GetNeuronById(string id, string centralId, NeuronQuery neuronQuery, Guid subjectId, CancellationToken token = default(CancellationToken));
    }
}
