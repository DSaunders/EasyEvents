namespace EasyEvents.SampleWebApp.Events.AppEvents
{
    public class ThingHappenedEvent : AppEvent
    {
        public string TheThing { get; }

        public ThingHappenedEvent(string theThing)
        {
            TheThing = theThing;
        }
    }
}