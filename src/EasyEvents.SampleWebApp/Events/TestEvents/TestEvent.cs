using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.SampleWebApp.Events.TestEvents
{
    public class TestEvent : IEvent
    {
        public string Stream => "TestEvents";
    }
}