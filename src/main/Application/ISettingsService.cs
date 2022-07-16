using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Diary.Nucleus.Application
{
    public interface ISettingsService
    {
        string CortexGraphOutBaseUrl { get; }
        string EventSourcingInBaseUrl { get; }
        string EventSourcingOutBaseUrl { get; }
        string IdentityAccessInBaseUrl { get; }
        string IdentityAccessOutBaseUrl { get; }
        string SubscriptionsInBaseUrl { get; }
    }
}
