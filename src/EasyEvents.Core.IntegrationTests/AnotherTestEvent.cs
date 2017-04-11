namespace EasyEvents.Core.IntegrationTests
{
    using ClientInterfaces;

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