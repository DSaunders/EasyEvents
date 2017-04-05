namespace EasyEvents.Core.ClientInterfaces
{
    using System.Threading.Tasks;

    public interface IEventHandler<TEventType> where TEventType : IEvent
    {
        Task HandleEventAsync(TEventType @event);
    }
}
