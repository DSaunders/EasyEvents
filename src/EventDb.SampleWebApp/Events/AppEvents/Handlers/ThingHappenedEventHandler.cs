namespace EventDb.SampleWebApp.Events.AppEvents.Handlers
{
    using System.Threading.Tasks;
    using EventDb.Core.ClientInterfaces;

    internal class ThingHappenedEventHandler : IEventHandler<ThingHappenedEvent>
    {
        public async Task HandleEvent(ThingHappenedEvent @event)
        {
            Logger.LogMessage("Thing happened: " + @event.TheThing);
        }
    }
}