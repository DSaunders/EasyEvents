using System;
using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.Tests.TestEvents
{
    public class HasDateTimePropertyWithNoSetterEvent : IEvent
    {
        public string Stream => "TestStream";

        public DateTime? DateTime { get; }
    }
}