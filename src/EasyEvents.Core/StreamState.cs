namespace EasyEvents.Core
{
    using System.Collections.Generic;

    public class StreamState
    {
        private readonly Dictionary<string, Dictionary<string, object>> _streamState;

        public StreamState()
        {
            _streamState = new Dictionary<string, Dictionary<string, object>>();
        }

        public Dictionary<string, object> GetStreamState(string streamName)
        {
            if (!_streamState.ContainsKey(streamName))
                _streamState.Add(streamName, new Dictionary<string, object>());

            return _streamState[streamName];
        }
    }
}