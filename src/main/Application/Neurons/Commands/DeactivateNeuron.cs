﻿using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using System;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons.Commands
{
    public class DeactivateNeuron : ICommand
    {
        public DeactivateNeuron(Guid id, string userId, int expectedVersion)
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
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
            this.UserId = userId;            
            this.ExpectedVersion = expectedVersion;
        }

        public Guid Id { get; private set; }

        public string UserId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
