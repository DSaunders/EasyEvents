using System;

namespace EasyEvents.Core.Exceptions
{
    public class EventHandlerException : Exception
    {
        public EventHandlerException(string message) : base(message)
        {
            
        }
    }
}