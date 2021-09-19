using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Diary.Nucleus.Application
{
    public interface ISettingsService
    {
        string CortexInBaseUrl { get; }
        string CortexOutBaseUrl { get; }
        string CortexGraphOutBaseUrl { get; }
        string EventSourcingOutBaseUrl { get; }
        string TagInBaseUrl { get; }
        string TagOutBaseUrl { get; }
        string AggregateInBaseUrl { get; }
        string AggregateOutBaseUrl { get; }
        string ExternalReferenceInBaseUrl { get; }
        string ExternalReferenceOutBaseUrl { get; }
        string IdentityAccessInBaseUrl { get; }
        string IdentityAccessOutBaseUrl { get; }
    }
}
