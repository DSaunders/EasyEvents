namespace EasyEvents.Core.ClientInterfaces
{
    using Newtonsoft.Json;

    public interface IEvent
    {
        [JsonIgnore]
        string Stream { get; }
    }
}