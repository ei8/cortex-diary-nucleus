using System;
using ei8.Cortex.Diary.Nucleus.Application;
using ei8.Cortex.Diary.Nucleus.Port.Adapter.Common;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.IO.Process.Services
{
    public class SettingsService : ISettingsService
    {
        public string CortexInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CortexInBaseUrl);

        public string CortexOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CortexOutBaseUrl);

        public string CortexGraphOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CortexGraphOutBaseUrl);

        public string EventSourcingOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.EventSourcingOutBaseUrl);

        public string TagInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TagInBaseUrl);

        public string TagOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TagOutBaseUrl);

        public string AggregateInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.AggregateInBaseUrl);

        public string AggregateOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.AggregateOutBaseUrl);

        public string IdentityAccessInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.IdentityAccessInBaseUrl);

        public string IdentityAccessOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.IdentityAccessOutBaseUrl);
    }
}
