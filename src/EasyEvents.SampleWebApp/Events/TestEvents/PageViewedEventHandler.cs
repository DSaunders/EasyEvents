using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.SampleWebApp.Events.TestEvents
{
    internal class PageViewedEventHandler : IEventHandler<PageViewedEvent>
    {
        public Task HandleEventAsync(PageViewedEvent @event)
        {
            Logger.LogMessage("Page viewed: " + @event.PageName);
            return Task.FromResult(0);
        }
    }
}