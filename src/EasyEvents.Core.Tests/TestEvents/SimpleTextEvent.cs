namespace EasyEvents.Core.Tests.TestEvents
{
    using ClientInterfaces;

    public class SimpleTextEvent : IEvent
    {
        public string Stream => "TestStream";

        public string SomeTestValue { get; }

        public SimpleTextEvent(string someTestValue)
        {
            SomeTestValue = someTestValue;
        }
    }
}