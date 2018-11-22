using Newtonsoft.Json;

namespace EasyEvents.Core.ClientInterfaces
{
    public interface IEvent
    {
        [JsonIgnore]
        string Stream { get; }
    }
}