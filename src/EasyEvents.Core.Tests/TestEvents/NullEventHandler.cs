using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;

namespace EasyEvents.Core.Tests.TestEvents
{
    public class NullEventHandler : IEventHandler<NullEvent>
    {
        public async Task HandleEventAsync(NullEvent @event)
        {
        }
    }
}