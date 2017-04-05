namespace EasyEvents.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Processors
    {
        private readonly Dictionary<string, IList<Func<Dictionary<string, object>, object, Task>>> _processors;

        public Processors()
        {
            _processors = new Dictionary<string, IList<Func<Dictionary<string, object>, object, Task>>>();
        }

        public IList<Func<Dictionary<string, object>, object, Task>> GetProcessorsForStream(string streamName)
        {
            return _processors.ContainsKey(streamName) 
                ? _processors[streamName] 
                : new List<Func<Dictionary<string, object>, object, Task>>();
        }

        public void AddProcessorForStream(string streamName, Func<Dictionary<string, object>, object, Task> processor)
        {
            if (!_processors.ContainsKey(streamName))
                _processors.Add(streamName, new List<Func<Dictionary<string, object>, object, Task>>());

            _processors[streamName].Add(processor);
        }
    }
}