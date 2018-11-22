using System;
using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.Tests.TestEvents
{
    public class HasDateTimePropertyEvent : IEvent
    {
        public string Stream => "TestStream";
        public DateTime DateTime { get; set; }
    }
}