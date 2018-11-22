using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.SampleWebApp.Events.AppEvents.Handlers
{
    internal class AppStartedEventHandler : IEventHandler<AppStartedEvent>
    {
        public Task HandleEventAsync(AppStartedEvent @event)
        {
            Logger.LogMessage("App Started");
            return Task.FromResult(0);
        }
    }
}