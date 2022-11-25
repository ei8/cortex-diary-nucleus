using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Nucleus.Application.Access
{
    public interface IAccessApplicationService
    {
        Task CreateNeuronAccessRequest(Guid neuronId, Guid userNeuronId, CancellationToken token = default);
    }
}
