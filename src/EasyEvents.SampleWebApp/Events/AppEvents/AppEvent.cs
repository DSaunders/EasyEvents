namespace EasyEvents.SampleWebApp.Events.AppEvents
{
    using Core.ClientInterfaces;

    /// <summary>
    /// This is the aggregate, and therefor defines the stream name.
    /// All events that relate to this aggregate derive from this, and will be on the same stream
    /// </summary>
    internal class AppEvent : IEvent
    {
        public string Stream => "AppEvents";
    }
}