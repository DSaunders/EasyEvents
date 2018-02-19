namespace EasyEvents.Core.Tests.TestEvents
{
    using System;
    using ClientInterfaces;

    public class HasDateTimePropertyWithNoSetterEvent : IEvent
    {
        public string Stream => "TestStream";

        public DateTime? DateTime { get; }
    }
}