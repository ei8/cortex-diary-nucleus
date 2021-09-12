using CQRSlite.Commands;
using Newtonsoft.Json;
using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands
{
    public class CreateNeuron : ICommand
    {
        public CreateNeuron(Guid id, string tag, Guid? regionId, string externalReferenceUrl, string userId)
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
            AssertionConcern.AssertArgumentNotNull(tag, nameof(tag));

            this.Id = id;            
            this.Tag = tag;
            this.RegionId = regionId;
            this.ExternalReferenceUrl = externalReferenceUrl;
            this.UserId = userId;
        }

        public Guid Id { get; private set; }
        
        public string Tag { get; private set; }

        public Guid? RegionId { get; private set; }

        public string ExternalReferenceUrl { get; private set; }

        public string UserId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
