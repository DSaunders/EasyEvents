namespace EasyEvents.Core.Configuration
{
    using System;
    using Stores;
    using Stores.InMemory;

    public class EasyEventsConfiguration
    {
        public IStore Store { get; set; }
        public Func<Type, object> HandlerFactory { get; set; }

        public EasyEventsConfiguration()
        {
            Store = new InMemoryEventStore();
        }
    }
}