namespace EasyEvents.SampleWebApp.Events.AppEvents.Handlers
{
    using System.Threading.Tasks;
    using Core.ClientInterfaces;

    internal class AppStartedEventHandler : IEventHandler<AppStartedEvent>
    {
        public Task HandleEventAsync(AppStartedEvent @event)
        {
            Logger.LogMessage("App Started");
            return Task.FromResult(0);
        }
    }
}