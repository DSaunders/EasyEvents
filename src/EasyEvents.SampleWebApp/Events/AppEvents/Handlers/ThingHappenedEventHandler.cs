namespace EasyEvents.SampleWebApp.Events.AppEvents.Handlers
{
    using System.Threading.Tasks;
    using Core.ClientInterfaces;

    internal class ThingHappenedEventHandler : IEventHandler<ThingHappenedEvent>
    {
        public async Task HandleEventAsync(ThingHappenedEvent @event)
        {
            Logger.LogMessage("Thing happened: " + @event.TheThing);
        }
    }
}