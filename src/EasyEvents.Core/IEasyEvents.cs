namespace EasyEvents.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClientInterfaces;
    using Configuration;

    public interface IEasyEvents
    {
        void Configure(EasyEventsConfiguration config);
        Task RaiseEventAsync(IEvent @event);
        Task ReplayAllEventsAsync();
        void AddProcessorForStream(string streamName, Func<Dictionary<string,object>, object, Task> processor);
        bool IsReplayingEvents { get; }
    }
}