using CQRSlite.Commands;
using Nancy;
using Nancy.Security;
using neurUL.Common.Domain.Model;
using ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands;
using ei8.Cortex.Diary.Nucleus.Port.Adapter.Common;
using System;
using System.Linq;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.In.Api
{
    public class NeuronModule : NancyModule
    {
        public NeuronModule(ICommandSender commandSender) : base("/nuclei/d23/neurons")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        false,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            Guid? regionId = null;

                            if (bodyAsDictionary.ContainsKey("RegionId"))
                                if (Guid.TryParse(bodyAsObject.RegionId.ToString(), out Guid tempRegionId))
                                    regionId = tempRegionId;

                            return new CreateNeuron(
                                Guid.Parse(bodyAsObject.Id.ToString()),
                                bodyAsObject.Tag.ToString(),
                                regionId,
                                bodyAsObject.UserId.ToString()
                                );                            
                        },
                        "Id",
                        "Tag",                        
                        "UserId"
                    );
            }
            );

            this.Patch("/{neuronId}", async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new ChangeNeuronTag(
                                Guid.Parse(parameters.neuronId),
                                bodyAsObject.Tag.ToString(),
                                bodyAsObject.UserId.ToString(),
                                expectedVersion
                                );
                        },
                        "Tag",
                        "UserId"
                    );
            }
            );

            this.Delete("/{neuronId}", async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new DeactivateNeuron(
                                Guid.Parse(parameters.neuronId),
                                bodyAsObject.UserId.ToString(),
                                expectedVersion
                                );
                        },
                        "UserId"
                    );
            }
            );
        }
    }
}
