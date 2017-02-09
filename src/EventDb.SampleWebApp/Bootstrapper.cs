namespace EventDb.SampleWebApp
{
    using Events.AppEvents;
    using Events.AppEvents.Handlers;
    using EventDb.Core;
    using EventDb.Core.ClientInterfaces;
    using EventDb.Core.Configuration;
    using EventDb.Core.Stores;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.TinyIoc;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            container.Register<IEventHandler<AppStartedEvent>, AppStartedEventHandler>();
            container.Register<IEventHandler<ThingHappenedEvent>, ThingHappenedEventHandler>();

            var eventDb = container.Resolve<IEventDb>();

            eventDb.Configure(new EventDbConfiguration
            {
                EventStore = new SqlEventStore("server=.;database=test;Integrated Security=true;"),
                HandlerFactory = type => container.Resolve(type)
            });

            eventDb.AddProcessorForStream(new AppEvent().Stream, async (c, e) =>
            {
                if (e.GetType() == typeof(ThingHappenedEvent) &&
                    ((ThingHappenedEvent)e).TheThing == "broke")
                {
                    await eventDb.RaiseEventAsync(new ThingHappenedEvent("Ooooh noooo!!"));
                }
            });

            eventDb.ReplayAllEventsAsync().Wait();
            eventDb.RaiseEventAsync(new AppStartedEvent()).Wait();
        }
    }
}