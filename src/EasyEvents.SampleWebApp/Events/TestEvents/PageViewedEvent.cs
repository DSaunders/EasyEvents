namespace EasyEvents.SampleWebApp.Events.TestEvents
{
    public class PageViewedEvent : TestEvent
    {
        public string PageName { get; }

        public PageViewedEvent(string pageName)
        {
            PageName = pageName;
        }
    }
}