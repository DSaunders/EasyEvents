namespace EventDb.Core.ClientInterfaces
{
    using System.Threading.Tasks;

    public interface IEventHandler<TEventType> where TEventType : IEvent
    {
        Task HandleEvent(TEventType @event);
    }
}
