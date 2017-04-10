namespace EasyEvents.Core.IntegrationTests
{
    using ClientInterfaces;
    public class TestEvent : IEvent
    {
        public string Stream => "test-stream";

        public string TestValue { get; }

        public TestEvent(string testValue)
        {
            TestValue = testValue;
        }
    }
}