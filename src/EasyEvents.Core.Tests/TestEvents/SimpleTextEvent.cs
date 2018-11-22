using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.Tests.TestEvents
{
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