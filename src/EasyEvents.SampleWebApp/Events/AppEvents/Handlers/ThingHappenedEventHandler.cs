using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.SampleWebApp.Events.AppEvents.Handlers
{
    internal class ThingHappenedEventHandler : IEventHandler<ThingHappenedEvent>
    {
        public Task HandleEventAsync(ThingHappenedEvent @event)
        {
            Logger.LogMessage("Thing happened: " + @event.TheThing);
            return Task.FromResult(0);
        }
    }
}