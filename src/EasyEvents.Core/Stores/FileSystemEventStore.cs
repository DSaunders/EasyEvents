namespace EasyEvents.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ClientInterfaces;
    using Newtonsoft.Json;

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

        private class EventStreams : IDisposable
        {
            private class EventStreamCollectionItem
            {
                public StreamReader Reader { get; }
                public FileSystemEventRecord NextEvent { get; set; }

                public EventStreamCollectionItem(StreamReader reader)
                {
                    Reader = reader;
                }
            }

            private readonly List<EventStreamCollectionItem> _nextEventsByStream;

            public EventStreams(IEnumerable<StreamReader> readers)
            {
                // Give each stream a unique ID so we can read each string later
                _nextEventsByStream = readers.Select(r => new EventStreamCollectionItem(r)).ToList();

                RefreshNextEventsForStreams().Wait();
            }

            public async Task<FileSystemEventRecord> GetNextEventInChronologicalOrder()
            {
                // Get all next events
                EventStreamCollectionItem minEvent = null;
                foreach (var nextEvent in _nextEventsByStream)
                {
                    if (minEvent == null || minEvent.NextEvent.CreatedDateTime > nextEvent.NextEvent.CreatedDateTime)
                        minEvent = nextEvent;
                }

                // We've used up all events on all streams
                if (minEvent == null)
                    return null;

                // Remove this event
                var returnVal = minEvent.NextEvent;
                minEvent.NextEvent = null;
                await RefreshNextEventsForStreams().ConfigureAwait(false);

                return returnVal;
            }

            public void Dispose()
            {
                foreach (var reader in _nextEventsByStream)
                    reader.Reader.Dispose();
            }

            private async Task RefreshNextEventsForStreams()
            {
                // Get the first set of events
                for (var i = _nextEventsByStream.Count - 1; i >= 0; i--)
                {
                    // We've already loaded the next event
                    if (_nextEventsByStream[i].NextEvent != null)
                        continue;

                    var nextEvent = await _nextEventsByStream[i].Reader.ReadLineAsync().ConfigureAwait(false);
                    if (nextEvent == null)
                    {
                        // Nothing left in the file, close stream and forget about this file
                        _nextEventsByStream[i].Reader.Dispose();
                        _nextEventsByStream.Remove(_nextEventsByStream[i]);
                        continue;
                    }

                    var record = nextEvent.Split('\t');
                    _nextEventsByStream[i].NextEvent = new FileSystemEventRecord(record[0], record[1], DateTime.Parse(record[2]));
                }
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

            var record = $"{@event.GetType().AssemblyQualifiedName}\t{JsonConvert.SerializeObject(@event)}\t{DateTime.UtcNow:o}";
            var path = Path.Combine(_folderPath, @event.Stream + ".stream.txt");

            File.AppendAllLines(path, new[] { record });

            return EventHandler(@event);
        }

        public async Task ReplayAllEvents()
        {
            CheckCreateEventsFolder();

            var files = Directory.GetFiles(_folderPath, "*.stream.txt", SearchOption.TopDirectoryOnly);

            // Open all streams at once
            using (var s = new EventStreams(files.Select(File.OpenText)))
            {
                FileSystemEventRecord e;
                while ((e = await s.GetNextEventInChronologicalOrder().ConfigureAwait(false)) != null)
                {
                    var type = GetEventType(e.EventName);
                    if (type == null)
                        continue;

                    var payload = (IEvent)JsonConvert.DeserializeObject(e.EventPayload, type);
                    await EventHandler(payload).ConfigureAwait(false);
                }
            }

        }

        private Type GetEventType(string eventName)
        {
            return Type.GetType(eventName);
        }

        private void CheckCreateEventsFolder()
        {
            if (_eventsFolderChecked)
                return;

            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);

            _eventsFolderChecked = true;
        }
    }
}