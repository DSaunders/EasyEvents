namespace EasyEvents.SampleWebApp
{
    using Core;
    using Core.ClientInterfaces;
    using Core.Configuration;
    using Core.Stores.FileSystem;
    using Events.AppEvents;
    using Events.AppEvents.Handlers;
    using Events.TestEvents;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.TinyIoc;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            container.Register<IEasyEvents, EasyEvents>().AsSingleton();
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