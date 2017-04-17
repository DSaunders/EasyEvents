namespace EasyEvents.Core.Tests.TestEvents
{
    using System;
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

    public class HasDateTimePropertyEvent : IEvent
    {
        public string Stream => "TestStream";
        public DateTime DateTime { get; set; }
    }

        public class HasDateTimePropertyWithIncorrectTypeEvent : IEvent
    {
        public string Stream => "TestStream";
        public string DateTime { get; set; }
    }

            public class HasDateTimePropertyWithNoSetterEvent : IEvent
    {
        public string Stream => "TestStream";
        public string DateTime { get; set; }
    }
}