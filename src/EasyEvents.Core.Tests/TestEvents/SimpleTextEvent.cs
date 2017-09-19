namespace EasyEvents.Core.Tests.TestEvents
{
    using ClientInterfaces;

    public class SimpleTextEvent : IEvent
    {
        public string Stream { get; set; }

        public string SomeTestValue { get; }

        public SimpleTextEvent(string someTestValue, string stream = "TestStream")
        {
            SomeTestValue = someTestValue;
            Stream = stream;
        }
    }
}