using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.Tests.TestEvents
{
    public class NullEvent : IEvent
    {
        public string Stream => "TestStream";

    }
}