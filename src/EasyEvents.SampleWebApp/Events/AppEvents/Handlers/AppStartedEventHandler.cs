namespace EasyEvents.SampleWebApp.Events.AppEvents.Handlers
{
    using System.Threading.Tasks;
    using Core.ClientInterfaces;

    internal class AppStartedEventHandler : IEventHandler<AppStartedEvent>
    {
        public async Task HandleEventAsync(AppStartedEvent @event)
        {
            Logger.LogMessage("App Started");
        }
    }
}