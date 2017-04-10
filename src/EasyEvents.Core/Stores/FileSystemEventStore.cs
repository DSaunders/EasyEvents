namespace EasyEvents.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ClientInterfaces;
    using Newtonsoft.Json;

    /// <summary>
    /// A horribly inefficient way of storing events on the file system, this will do for
    /// debugging etc. but needs some love
    /// </summary>

    public class FileSystemEventStore : IStore
    {
        private class FileSystemEventRecord
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

        private bool _eventsFolderChecked;
        private readonly string _folderPath;

        public Func<IEvent, Task> EventHandler { get; set; }

        public FileSystemEventStore()
        {
            _folderPath = Path.Combine(Directory.GetCurrentDirectory(), "_events");
        }

        public FileSystemEventStore(string eventsFolder)
        {
            _folderPath = Path.Combine(eventsFolder);
        }

        public Task RaiseEventAsync(IEvent @event)
        {
            CheckCreateEventsFolder();

            var record = new
                FileSystemEventRecord(@event.GetType().AssemblyQualifiedName, JsonConvert.SerializeObject(@event), DateTime.UtcNow);

            var path = Path.Combine(_folderPath, @event.Stream + ".stream.txt");

            // Open the file for this stream
            File.AppendAllLines(path, new[] { JsonConvert.SerializeObject(record) });
            return EventHandler(@event);
        }

        public async Task ReplayAllEvents()
        {
            CheckCreateEventsFolder();

            var files = Directory.GetFiles(_folderPath, "*.stream.txt", SearchOption.TopDirectoryOnly);

            var allEvents = new List<FileSystemEventRecord>();
            foreach (var file in files)
            {
                using (var stream = File.OpenText(file))
                {
                    string s;
                    while ((s = await stream.ReadLineAsync()) != null)
                    {
                        allEvents.Add(JsonConvert.DeserializeObject<FileSystemEventRecord>(s));
                    }
                }
            }

            var orderedEvents = allEvents.OrderBy(e => e.CreatedDateTime);

            foreach (var e in orderedEvents)
            {
                var type = GetEventType(e.EventName);
                if (type == null)
                    continue;

                var payload = (IEvent)JsonConvert.DeserializeObject(e.EventPayload, type);
                await EventHandler(payload).ConfigureAwait(false);
            }
        }

        private Type GetEventType(string eventName)
        {
            return Type.GetType(eventName);
        }

        private void CheckCreateEventsFolder()
        {
            if (!_eventsFolderChecked)
            {
                if (!Directory.Exists(_folderPath))
                {
                    Directory.CreateDirectory(_folderPath);
                }
                _eventsFolderChecked = true;
            }
        }

    }
}