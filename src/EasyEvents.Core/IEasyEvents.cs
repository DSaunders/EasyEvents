using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;
using EasyEvents.Core.Configuration;

namespace EasyEvents.Core
{
    public interface IEasyEvents
    {
        void Configure(EasyEventsConfiguration config);
        Task RaiseEventAsync(IEvent @event);
        Task ReplayAllEventsAsync();
        void AddProcessorForStream(string streamName, Func<Dictionary<string,object>, object, Task> processor);
        bool IsReplayingEvents { get; }
    }
}