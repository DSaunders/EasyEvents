namespace EasyEvents.Core.Tests.TestEvents
{
    using System.Threading.Tasks;
    using ClientInterfaces;

    public class NullEventHandler : IEventHandler<NullEvent>
    {
        public async Task HandleEventAsync(NullEvent @event)
        {
        }
    }
}