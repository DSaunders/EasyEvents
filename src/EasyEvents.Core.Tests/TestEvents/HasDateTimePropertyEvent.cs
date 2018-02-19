namespace EasyEvents.Core.Tests.TestEvents
{
    using System;
    using ClientInterfaces;

    public class HasDateTimePropertyEvent : IEvent
    {
        public string Stream => "TestStream";
        public DateTime DateTime { get; set; }
    }
}