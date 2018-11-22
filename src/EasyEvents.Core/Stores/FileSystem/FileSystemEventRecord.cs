using System;

namespace EasyEvents.Core.Stores.FileSystem
{
    internal class FileSystemEventRecord
    {
        public string EventName { get; }
        public string EventPayload { get; }
        public DateTime CreatedDateTime { get; }

        public FileSystemEventRecord(string eventName, string eventPayload, DateTime createdDateTime)
        {
            EventName = eventName;
            EventPayload = eventPayload;
            CreatedDateTime = createdDateTime;
        }
    }
}