namespace EasyEvents.SampleWebApp.Events.TestEvents
{
    using System.Threading.Tasks;
    using Core.ClientInterfaces;

    internal class PageViewedEventHandler : IEventHandler<PageViewedEvent>
    {
        public Task HandleEventAsync(PageViewedEvent @event)
        {
            Logger.LogMessage("Page viewed: " + @event.PageName);
            return Task.FromResult(0);
        }
    }
}