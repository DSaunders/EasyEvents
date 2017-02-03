namespace EventDb.Core.Stores
{
    using System;
    using System.Threading.Tasks;
    using ClientInterfaces;

    public interface IStore
    {
        Func<IEvent, Task> EventHandler { get; set; }
        Task RaiseEventAsync(IEvent @event);
        Task ReplayAllEvents();
    }
}