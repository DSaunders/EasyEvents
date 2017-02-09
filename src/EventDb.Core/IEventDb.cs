namespace EventDb.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClientInterfaces;
    using Configuration;

    public interface IEventDb
    {
        void Configure(EventDbConfiguration config);
        Task RaiseEventAsync(IEvent @event);
        Task ReplayAllEventsAsync();
        void AddProcessorForStream(string streamName, Func<Dictionary<string,object>, object, Task> processor);
    }
}