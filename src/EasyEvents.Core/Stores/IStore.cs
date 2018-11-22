using System;
using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.Stores
{
    public interface IStore
    {
        Func<IEvent, Task> EventHandler { get; set; }
        Task RaiseEventAsync(IEvent @event);
        Task ReplayAllEvents();
    }
}