namespace EasyEvents.SampleWebApp.Events.AppEvents.Handlers
{
    using System.Threading.Tasks;
    using Core.ClientInterfaces;

    internal class ThingHappenedEventHandler : IEventHandler<ThingHappenedEvent>
    {
        public Task HandleEventAsync(ThingHappenedEvent @event)
        {
            Logger.LogMessage("Thing happened: " + @event.TheThing);
            return Task.FromResult(0);
        }
    }
}