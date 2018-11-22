using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.IntegrationTests
{
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