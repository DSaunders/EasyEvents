using EasyEvents.Core;
using EasyEvents.Core.ClientInterfaces;
using EasyEvents.Core.Configuration;
using EasyEvents.Core.Stores.FileSystem;
using EasyEvents.SampleWebApp.Events.AppEvents;
using EasyEvents.SampleWebApp.Events.AppEvents.Handlers;
using EasyEvents.SampleWebApp.Events.TestEvents;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace EasyEvents.SampleWebApp
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            container.Register<IEasyEvents, Core.EasyEvents>().AsSingleton();
            container.Register<IEventHandler<AppStartedEvent>, AppStartedEventHandler>();
            container.Register<IEventHandler<ThingHappenedEvent>, ThingHappenedEventHandler>();
            container.Register<IEventHandler<PageViewedEvent>, PageViewedEventHandler>();

            var events = container.Resolve<IEasyEvents>();

            events.Configure(new EasyEventsConfiguration
            {
                Store = new FileSystemEventStore(),
                //Store = new SqlEventStore("server=.;database=test;Integrated Security=true;"),
                HandlerFactory = type => container.CanResolve(type) ? container.Resolve(type) : null
            });

            events.AddProcessorForStream(new AppEvent().Stream, async (c, e) =>
            {
                if (e is ThingHappenedEvent &&
                    ((ThingHappenedEvent)e).TheThing == "broke")
                {
                    await events.RaiseEventAsync(new ThingHappenedEvent("Ooooh noooo!!"));
                }
            });

            events.ReplayAllEventsAsync().Wait();
            events.RaiseEventAsync(new AppStartedEvent()).Wait();
        }
    }
}