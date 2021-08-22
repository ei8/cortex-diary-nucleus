using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands
{
    public class ChangeNeuronUrl : ICommand
    {
        public ChangeNeuronUrl(Guid id, string newUrl, string userId, int expectedVersion)
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
            AssertionConcern.AssertArgumentNotNull(newUrl, nameof(newUrl));
            AssertionConcern.AssertArgumentNotEmpty(
                userId,
                Messages.Exception.InvalidUserId,
                nameof(userId)
                );
            AssertionConcern.AssertArgumentValid(
                i => i >= 1,
                expectedVersion,
                Messages.Exception.InvalidExpectedVersion,
                nameof(expectedVersion)
                );

            this.Id = id;            
            this.NewUrl = newUrl;
            this.UserId = userId;
            this.ExpectedVersion = expectedVersion;
        }

        public Guid Id { get; private set; }

        public string NewUrl { get; private set; }

        public string UserId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
