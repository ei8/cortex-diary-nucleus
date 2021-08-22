using CQRSlite.Commands;
using Nancy;
using Nancy.Security;
using neurUL.Cortex.Common;
using System;
using ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands;
using ei8.Cortex.Diary.Nucleus.Port.Adapter.Common;
using neurUL.Common.Api;
using System.Collections.Generic;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.In.Api
{
    public class TerminalModule : NancyModule
    {
        public TerminalModule(ICommandSender commandSender) : base("/nuclei/d23/terminals")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        false,
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            TerminalModule.CreateTerminalFromDynamic(bodyAsObject, bodyAsDictionary, out Guid terminalId, out Guid presynapticNeuronId, 
                                out Guid postsynapticNeuronId, out NeurotransmitterEffect effect, out float strength, out string url, out string userId);

                            await commandSender.Send(new CreateTerminal(terminalId, presynapticNeuronId, postsynapticNeuronId, 
                                effect, strength, url, userId));
                        },
                        NeuronModule.ConcurrencyExceptionSetter,
                        new string[0],
                        "Id",
                        "PresynapticNeuronId",
                        "PostsynapticNeuronId",
                        "Effect",
                        "Strength",
                        "UserId"
                    );
            }
            );

            this.Delete("/{terminalId}", async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            await commandSender.Send(new DeactivateTerminal(
                                Guid.Parse(parameters.terminalId),
                                bodyAsObject.UserId.ToString(),
                                expectedVersion
                                ));
                        },
                        NeuronModule.ConcurrencyExceptionSetter,
                        new string[0],
                        "UserId"
                    );
                }
            );
        }

        private static void CreateTerminalFromDynamic(dynamic dynamicTerminal, Dictionary<string, object> bodyAsDictionary, out Guid terminalId, out Guid presynapticNeuronId, 
            out Guid postsynapticNeuronId, out NeurotransmitterEffect effect, out float strength, out string url, out string userId)
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
            url = bodyAsDictionary.ContainsKey("Url") ? dynamicTerminal.Url.ToString() : null;
            userId = dynamicTerminal.UserId.ToString();
        }
    }
}
