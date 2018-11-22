using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;
using Newtonsoft.Json;

namespace EasyEvents.Core.Stores.FileSystem
{
    public class FileSystemEventStore : IStore
    {
        private readonly EventTypeCache _typeCache;

        private bool _eventsFolderChecked;
        private readonly string _folderPath;

        public Func<IEvent, Task> EventHandler { get; set; }

        public FileSystemEventStore(string eventsFolder = null)
        {
            _typeCache = new EventTypeCache();

            _folderPath = eventsFolder != null
                ? Path.Combine(eventsFolder)
                : Path.Combine(Directory.GetCurrentDirectory(), "_events");
        }

        public Task RaiseEventAsync(IEvent @event)
        {
            CheckCreateEventsFolder();

            var record = $"{@event.GetType().Name}\t{JsonConvert.SerializeObject(@event)}\t{DateTime.UtcNow:o}";
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
                    var type = _typeCache.GetEventTypeFromName(e.EventName);
                    if (type == null)
                        continue;

                    var payload = (IEvent)JsonConvert.DeserializeObject(e.EventPayload, type);
                    await EventHandler(payload).ConfigureAwait(false);
                }
            }
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