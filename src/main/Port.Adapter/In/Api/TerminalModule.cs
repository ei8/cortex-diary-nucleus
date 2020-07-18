using CQRSlite.Commands;
using Nancy;
using Nancy.Security;
using neurUL.Cortex.Common;
using System;
using ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands;
using ei8.Cortex.Diary.Nucleus.Port.Adapter.Common;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.In.Api
{
    public class TerminalModule : NancyModule
    {
        public TerminalModule(ICommandSender commandSender) : base("/nuclei/d23/terminals")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        false,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            TerminalModule.CreateTerminalFromDynamic(bodyAsObject, out Guid terminalId, out Guid presynapticNeuronId, 
                                out Guid postsynapticNeuronId, out NeurotransmitterEffect effect, out float strength, out Guid subjectId);

                            return new CreateTerminal(terminalId, presynapticNeuronId, postsynapticNeuronId, 
                                effect, strength, subjectId);
                        },
                        "Id",
                        "PresynapticNeuronId",
                        "PostsynapticNeuronId",
                        "Effect",
                        "Strength",
                        "SubjectId"
                    );
            }
            );

            this.Delete("/{terminalId}", async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new DeactivateTerminal(
                                Guid.Parse(parameters.terminalId),
                                Guid.Parse(bodyAsObject.SubjectId.ToString()),
                                expectedVersion
                                );
                        },
                        "SubjectId"
                    );
                }
            );
        }

        private static void CreateTerminalFromDynamic(dynamic dynamicTerminal, out Guid terminalId, out Guid presynapticNeuronId, 
            out Guid postsynapticNeuronId, out NeurotransmitterEffect effect, out float strength, out Guid subjectId)
        {
            terminalId = Guid.Parse(dynamicTerminal.Id.ToString());
            presynapticNeuronId = Guid.Parse(dynamicTerminal.PresynapticNeuronId.ToString());
            postsynapticNeuronId = Guid.Parse(dynamicTerminal.PostsynapticNeuronId.ToString());
            string ne = dynamicTerminal.Effect.ToString();
            if (Enum.IsDefined(typeof(NeurotransmitterEffect), (int.TryParse(ne, out int ine) ? (object)ine : ne)))
                effect = (NeurotransmitterEffect)Enum.Parse(typeof(NeurotransmitterEffect), dynamicTerminal.Effect.ToString());
            else
                throw new ArgumentOutOfRangeException("Effect", $"Specified NeurotransmitterEffect value of '{dynamicTerminal.Effect.ToString()}' was invalid");
            strength = float.Parse(dynamicTerminal.Strength.ToString());
            subjectId = Guid.Parse(dynamicTerminal.SubjectId.ToString());
        }
    }
}
