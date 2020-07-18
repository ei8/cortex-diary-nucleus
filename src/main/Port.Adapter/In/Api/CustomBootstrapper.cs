using CQRSlite.Commands;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using neurUL.Cortex.Client.In;
using System;
using ei8.Cortex.Diary.Nucleus.Application;
using ei8.Cortex.Diary.Nucleus.Application.Neurons;
using ei8.Cortex.Diary.Nucleus.Port.Adapter.IO.Process.Services;
using ei8.Data.Aggregate.Client.In;
using ei8.Data.Tag.Client.In;
using ei8.Cortex.IdentityAccess.Client.Out;
using ei8.Cortex.Graph.Client;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.In.Api
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        public CustomBootstrapper()
        {
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            // create a singleton instance which will be reused for all calls in current request
            var ipb = new Router();
            container.Register<ICommandSender, Router>(ipb);
            container.Register<IHandlerRegistrar, Router>(ipb);
            container.Register<IRequestProvider, RequestProvider>();
            container.Register<INeuronClient, HttpNeuronClient>();
            container.Register<ITerminalClient, HttpTerminalClient>();
            container.Register<ITagClient, HttpTagClient>();
            container.Register<IAggregateClient, HttpAggregateClient>();
            container.Register<ISettingsService, SettingsService>();
            container.Register<IValidationClient, HttpValidationClient>();
            container.Register<INeuronGraphQueryClient, HttpNeuronGraphQueryClient>();
            container.Register<NeuronCommandHandlers>();
            container.Register<TerminalCommandHandlers>();

            // TODO: necessary?
            var ticl = new TinyIoCServiceLocator(container);
            container.Register<IServiceProvider, TinyIoCServiceLocator>(ticl);
            var registrar = new RouteRegistrar(ticl);
            registrar.Register(typeof(NeuronCommandHandlers));

            ((TinyIoCServiceLocator)container.Resolve<IServiceProvider>()).SetRequestContainer(container);
        }
    }
}
