namespace EasyEvents.SampleWebApp.Events.AppEvents
{
    internal class ThingHappenedEvent : AppEvent
    {
        public string TheThing { get; }

        public ThingHappenedEvent(string theThing)
        {
            TheThing = theThing;
        }
    }
}