namespace EventDb.Core.Tests.TestEvents
{
    using System.Threading.Tasks;
    using ClientInterfaces;

    public class NullEventHandler : IEventHandler<NullEvent>
    {
        public async Task HandleEvent(NullEvent @event)
        {
        }
    }
}