namespace EasyEvents.Core.Exceptions
{
    using System;

    public class EventHandlerException : Exception
    {
        public EventHandlerException(string message) : base(message)
        {
            
        }
    }
}