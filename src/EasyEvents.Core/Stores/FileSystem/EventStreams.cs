namespace EasyEvents.Core.Stores.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    internal class EventStreams : IDisposable
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

        private readonly List<EventStreamCollectionItem> _nextEventForEachStream;

        public EventStreams(IEnumerable<StreamReader> eventStreamReaders)
        {
            _nextEventForEachStream = eventStreamReaders.Select(r => new EventStreamCollectionItem(r)).ToList();
        }

        public async Task<FileSystemEventRecord> GetNextEventInChronologicalOrder()
        {
            await RefreshNextEventsForStreams().ConfigureAwait(false);

            // Get all next events
            EventStreamCollectionItem streamWithNextEvent = null;
            foreach (var stream in _nextEventForEachStream)
            {
                if (streamWithNextEvent == null ||
                    streamWithNextEvent.NextEvent.CreatedDateTime > stream.NextEvent.CreatedDateTime)
                    streamWithNextEvent = stream;
            }

            // We've used up all events on all streams
            if (streamWithNextEvent == null)
                return null;

            // Remove this event once it's returned, so we load the next event
            //  on the next iteration
            var returnVal = streamWithNextEvent.NextEvent;
            streamWithNextEvent.NextEvent = null;

            return returnVal;
        }

        private async Task RefreshNextEventsForStreams()
        {
            // Get the first set of events
            for (var i = _nextEventForEachStream.Count - 1; i >= 0; i--)
            {
                // We've already loaded the next event
                if (_nextEventForEachStream[i].NextEvent != null)
                    continue;

                var nextEvent = await _nextEventForEachStream[i].Reader.ReadLineAsync().ConfigureAwait(false);
                if (nextEvent == null)
                {
                    // Nothing left in the file, close stream and forget about this file
                    _nextEventForEachStream[i].Reader.Dispose();
                    _nextEventForEachStream.Remove(_nextEventForEachStream[i]);
                    continue;
                }

                var record = nextEvent.Split('\t');
                _nextEventForEachStream[i].NextEvent = new FileSystemEventRecord(record[0], record[1],
                    DateTime.Parse(record[2]));
            }
        }

        public void Dispose()
        {
            foreach (var reader in _nextEventForEachStream)
                reader.Reader.Dispose();
        }
    }
}