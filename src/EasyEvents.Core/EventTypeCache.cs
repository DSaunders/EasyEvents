namespace EasyEvents.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ClientInterfaces;
    using Microsoft.Extensions.DependencyModel;

    public class EventTypeCache
    {
        private readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

        private bool _isPrimed = false;
        private readonly object _lock = new object();

        public Type GetEventTypeFromName(string name)
        {
            if (!_isPrimed)
            {
                lock (_lock)
                {
                    if (!_isPrimed)
                        PrimeCache();
                }
            }

            return _typeCache.TryGetValue(name, out var eventType)
                ? eventType
                : null;
        }

        private void PrimeCache()
        {
            var platform = Environment.OSVersion.Platform.ToString();
            var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform);

            foreach (var assemblyName in runtimeAssemblyNames)
            {
                var assembly = Assembly.Load(assemblyName);
                var iEventTypes = assembly
                    .ExportedTypes
                    .Where(t => typeof(IEvent).IsAssignableFrom(t));

                foreach (var eventType in iEventTypes)
                {
                    _typeCache.Add(eventType.Name, eventType);
                }
            }

            _isPrimed = true;
        }
    }
}