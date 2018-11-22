using System.Threading.Tasks;

namespace EasyEvents.Core.ClientInterfaces
{
    public interface IEventHandler<TEventType> where TEventType : IEvent
    {
        Task HandleEventAsync(TEventType @event);
    }
}
