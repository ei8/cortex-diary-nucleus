using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using ei8.Cortex.Diary.Nucleus.Application;
using ei8.Cortex.Diary.Nucleus.Application.Neurons;
using ei8.Cortex.Diary.Nucleus.Application.Notification;
using ei8.Cortex.Diary.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Cortex.Graph.Client;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.Out;
using ei8.Cortex.IdentityAccess.Client.Out;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.Out.Api
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        public CustomBootstrapper()
        {
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.Register<IRequestProvider, RequestProvider>();
            container.Register<ISettingsService, SettingsService>();
            container.Register<INeuronGraphQueryClient, HttpNeuronGraphQueryClient>();
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<INeuronQueryService, NeuronQueryService>();
            container.Register<IEventSerializer, EventSerializer>();
            container.Register<INotificationClient, HttpNotificationClient>();
            container.Register<INotificationApplicationService, NotificationApplicationService>();
        }
    }
}
