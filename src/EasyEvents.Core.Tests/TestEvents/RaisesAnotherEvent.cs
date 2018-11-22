using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.Tests.TestEvents
{
    public class RaisesAnotherEvent : IEvent
    {
        public string Stream => "test-stream";
    }
}
