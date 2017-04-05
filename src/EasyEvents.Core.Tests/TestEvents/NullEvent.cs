namespace EasyEvents.Core.Tests.TestEvents
{
    using ClientInterfaces;

    public class NullEvent : IEvent
    {
        public string Stream => "TestStream";

    }
}