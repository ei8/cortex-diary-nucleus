using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ei8.Cortex.Diary.Common;
using ei8.Cortex.Diary.Nucleus.Application.Neurons;
using neurUL.Common.Domain.Model;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.Out.Api
{
    public class NeuronModule : NancyModule
    {
        public NeuronModule(INeuronQueryService queryService) : base("/nuclei/d23/neurons")
        {
            this.Get("", async (parameters) =>
            {
                return await NeuronModule.ProcessRequest(async () =>
                {
                    var nv = await queryService.GetNeurons(NeuronModule.ExtractQuery(this.Request.Query), NeuronModule.GetSubjectId(this.Request));
                    return new TextResponse(JsonConvert.SerializeObject(nv));
                }
                );
            }
            );

            this.Get("/{neuronid:guid}", async (parameters) =>
            {
                return await NeuronModule.ProcessRequest(async () =>
                {
                    var nv = await queryService.GetNeuronById(parameters.neuronid, NeuronModule.ExtractQuery(this.Request.Query), NeuronModule.GetSubjectId(this.Request));
                    return new TextResponse(JsonConvert.SerializeObject(nv));
                }
                );
            }
            );

            this.Get("/{centralid:guid}/relatives", async (parameters) =>
            {
                return await NeuronModule.ProcessRequest(async () =>
                {
                    var nv = await queryService.GetNeurons(
                        parameters.centralid,
                        NeuronModule.ExtractQuery(this.Request.Query),
                        NeuronModule.GetSubjectId(this.Request)
                        );

                    return new TextResponse(JsonConvert.SerializeObject(nv));
                }
                );
            }
            );

            this.Get("/{centralid:guid}/relatives/{neuronid:guid}", async (parameters) =>
            {
                return await NeuronModule.ProcessRequest(async () =>
                {
                    var nv = await queryService.GetNeuronById(
                        parameters.neuronid,
                        parameters.centralid,
                        NeuronModule.ExtractQuery(this.Request.Query), 
                        NeuronModule.GetSubjectId(this.Request)
                        );
                    return new TextResponse(JsonConvert.SerializeObject(nv));
                }
                );
            }
            );
        }

        private static Guid GetSubjectId(Request value)
        {
            AssertionConcern.AssertArgumentValid(k => k, (bool) value.Query["subjectid"].HasValue, "Subject Id was not found.", "subjectid");

            return Guid.Parse(value.Query["subjectid"].ToString());
        }

        private static NeuronQuery ExtractQuery(dynamic query)
        {
            var nq = new NeuronQuery();
            nq.TagContains = NeuronModule.GetQueryArrayOrDefault(query, nameof(NeuronQuery.TagContains));
            nq.TagContainsNot = NeuronModule.GetQueryArrayOrDefault(query, nameof(NeuronQuery.TagContainsNot));
            nq.Id = NeuronModule.GetQueryArrayOrDefault(query, nameof(NeuronQuery.Id));
            nq.IdNot = NeuronModule.GetQueryArrayOrDefault(query, nameof(NeuronQuery.IdNot));
            nq.Postsynaptic = NeuronModule.GetQueryArrayOrDefault(query, nameof(NeuronQuery.Postsynaptic));
            nq.PostsynapticNot = NeuronModule.GetQueryArrayOrDefault(query, nameof(NeuronQuery.PostsynapticNot));
            nq.Presynaptic = NeuronModule.GetQueryArrayOrDefault(query, nameof(NeuronQuery.Presynaptic));
            nq.PresynapticNot = NeuronModule.GetQueryArrayOrDefault(query, nameof(NeuronQuery.PresynapticNot));
            nq.RelativeValues = query["relative"].HasValue ? (RelativeValues?)Enum.Parse(typeof(RelativeValues), query["relative"].ToString(), true) : null;
            nq.Limit = query["limit"].HasValue ? int.Parse(query["limit"].ToString()) : null;
            nq.NeuronActiveValues = query["nactive"].HasValue ? (ActiveValues?)Enum.Parse(typeof(ActiveValues), query["nactive"].ToString(), true) : null;
            nq.TerminalActiveValues = query["tactive"].HasValue ? (ActiveValues?)Enum.Parse(typeof(ActiveValues), query["tactive"].ToString(), true) : null;
            return nq;
        }

        private static IEnumerable<string> GetQueryArrayOrDefault(dynamic query, string parameterName)
        {
            var parameterNameExclamation = parameterName.Replace("Not", "!");
            return query[parameterName].HasValue ?
                query[parameterName].ToString().Split(",") :
                    query[parameterNameExclamation].HasValue ?
                    query[parameterNameExclamation].ToString().Split(",") :
                    null;
        }

        internal static async Task<Response> ProcessRequest(Func<Task<Response>> action)
        {
            var result = new Response { StatusCode = HttpStatusCode.OK };

            try
            {
                result = await action();
            }
            catch (Exception ex)
            {
                result = new TextResponse(HttpStatusCode.BadRequest, ex.ToString());
            }

            return result;
        }
    }
}
