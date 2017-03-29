namespace EasyEvents.Core.Configuration
{
    using System;
    using Stores;

    public class EasyEventsConfiguration
    {
        public IStore EventStore { get; set; }
        public Func<Type, object> HandlerFactory { get; set; }

        public EasyEventsConfiguration()
        {
            EventStore = new InMemoryEventStore();
        }
    }
}