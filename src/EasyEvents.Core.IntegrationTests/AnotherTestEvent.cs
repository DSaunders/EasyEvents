using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.IntegrationTests
{
    public class AnotherTestEvent : IEvent
    {
        public string Stream => "another-test-stream";

        public string TestValue { get; }

        public AnotherTestEvent(string testValue)
        {
            TestValue = testValue;
        }
    }
}