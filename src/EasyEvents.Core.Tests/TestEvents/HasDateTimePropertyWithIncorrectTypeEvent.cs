using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.Tests.TestEvents
{
    public class HasDateTimePropertyWithIncorrectTypeEvent : IEvent
    {
        public string Stream => "TestStream";
        public string DateTime { get; set; }
    }
}