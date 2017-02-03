namespace EventDb.Core.Configuration
{
    using System;
    using Stores;

    public class EventDbConfiguration
    {
        public IStore EventStore { get; set; }
        public Func<Type, object> HandlerFactory { get; set; }

        public EventDbConfiguration()
        {
            EventStore = new InMemoryEventStore();
        }
    }
}