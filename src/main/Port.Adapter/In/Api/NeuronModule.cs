﻿using CQRSlite.Commands;
using Nancy;
using ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands;
using System;
using neurUL.Common.Api;
using CQRSlite.Domain.Exception;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.In.Api
{
    public class NeuronModule : NancyModule
    {
        internal static readonly Func<Exception, HttpStatusCode, HttpStatusCode> ConcurrencyExceptionSetter = new Func<Exception, HttpStatusCode, HttpStatusCode>((ex, hsc) => { 
                            HttpStatusCode result = hsc;                   
                            if (ex is ConcurrencyException)
                                result = HttpStatusCode.Conflict;                            
                            return result;
                        });
        public NeuronModule(ICommandSender commandSender) : base("/nuclei/d23/neurons")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        false,
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            Guid? regionId = null;

                            if (bodyAsDictionary.ContainsKey("RegionId"))
                                if (Guid.TryParse(bodyAsObject.RegionId.ToString(), out Guid tempRegionId))
                                    regionId = tempRegionId;
                            
                            string url = null;

                            if (bodyAsDictionary.ContainsKey("Url"))
                                url = bodyAsObject.Url.ToString();

                            await commandSender.Send(new CreateNeuron(
                                Guid.Parse(bodyAsObject.Id.ToString()),
                                bodyAsObject.Tag.ToString(),
                                regionId,
                                url,
                                bodyAsObject.UserId.ToString()
                                )
                            );                            
                        },
                        ConcurrencyExceptionSetter,
                        new string[0],
                        "Id",
                        "Tag",                        
                        "UserId"
                    );
            }
            );

            this.Patch("/{neuronId}", async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            ICommand result = null;
                            if (bodyAsDictionary.ContainsKey("Tag"))
                                result = new ChangeNeuronTag(
                                    Guid.Parse(parameters.neuronId),
                                    bodyAsObject.Tag.ToString(),
                                    bodyAsObject.UserId.ToString(),
                                    expectedVersion
                                    );
                            else if (bodyAsDictionary.ContainsKey("Url"))
                                result = new ChangeNeuronUrl(
                                    Guid.Parse(parameters.neuronId),
                                    bodyAsObject.Url.ToString(),
                                    bodyAsObject.UserId.ToString(),
                                    expectedVersion
                                    );
                            await commandSender.Send(result);
                        },
                        ConcurrencyExceptionSetter,
                        new string[] { "Tag", "Url" },
                        "UserId"
                    );
            }
            );

            this.Delete("/{neuronId}", async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            await commandSender.Send(new DeactivateNeuron(
                                Guid.Parse(parameters.neuronId),
                                bodyAsObject.UserId.ToString(),
                                expectedVersion
                                ));
                        },
                        ConcurrencyExceptionSetter,
                        new string[0],
                        "UserId"
                    );
            }
            );
        }
    }
}
