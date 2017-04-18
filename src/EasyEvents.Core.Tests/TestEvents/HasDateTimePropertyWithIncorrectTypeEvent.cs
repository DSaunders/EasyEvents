namespace EasyEvents.Core.Tests.TestEvents
{
    using ClientInterfaces;
    public class HasDateTimePropertyWithIncorrectTypeEvent : IEvent
    {
        public string Stream => "TestStream";
        public string DateTime { get; set; }
    }
}