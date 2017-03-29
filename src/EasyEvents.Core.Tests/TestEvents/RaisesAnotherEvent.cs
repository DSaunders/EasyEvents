namespace EasyEvents.Core.Tests.TestEvents
{
    using ClientInterfaces;

    public class RaisesAnotherEvent : IEvent
    {
        public string Stream => "test-stream";
    }
}
