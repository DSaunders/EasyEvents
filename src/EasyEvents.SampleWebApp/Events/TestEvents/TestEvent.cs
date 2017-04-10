namespace EasyEvents.SampleWebApp.Events.TestEvents
{
    using Core.ClientInterfaces;

    public class TestEvent : IEvent
    {
        public string Stream => "TestEvents";
    }
}