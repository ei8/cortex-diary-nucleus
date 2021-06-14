using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Diary.Nucleus.Application.Neurons
{
    public struct Messages
    {
        public struct Exception
        {
            public const string InvalidId = "Id must not be equal to '00000000-0000-0000-0000-000000000000'.";
            public const string InvalidUserId = "User Id must not be null or empty.";
            public const string InvalidExpectedVersion = "Expected Version must be equal to or greater than '1'.";
        }
    }
}
