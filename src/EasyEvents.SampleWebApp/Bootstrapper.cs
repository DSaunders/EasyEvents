namespace EasyEvents.SampleWebApp
{
    using Core;
    using Core.ClientInterfaces;
    using Core.Configuration;
    using Core.Stores;
    using Events.AppEvents;
    using Events.AppEvents.Handlers;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.TinyIoc;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            container.Register<IEventHandler<AppStartedEvent>, AppStartedEventHandler>();
            container.Register<IEventHandler<ThingHappenedEvent>, ThingHappenedEventHandler>();

            var events = container.Resolve<IEasyEvents>();

            events.Configure(new EasyEventsConfiguration
            {
                Store = new SqlEventStore("server=.;database=test;Integrated Security=true;"),
                HandlerFactory = type => container.Resolve(type)
            });

            events.AddProcessorForStream(new AppEvent().Stream, async (c, e) =>
            {
                if (e.GetType() == typeof(ThingHappenedEvent) &&
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