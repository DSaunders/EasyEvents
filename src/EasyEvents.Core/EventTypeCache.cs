namespace EasyEvents.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
            foreach (var lib in DependencyContext.Default.RuntimeLibraries)
            {
                try
                {
                    var assembly = Assembly.Load(new AssemblyName(lib.Name));
                    var iEventTypes = assembly
                        .ExportedTypes
                        .Where(t => typeof(IEvent).IsAssignableFrom(t));

                    foreach (var eventType in iEventTypes)
                    {
                        _typeCache.Add(eventType.Name, eventType);
                    }

                }
                catch (FileNotFoundException ex)
                {
                    // ☹ Some of the core assemblies throw a FileNotFound exception when calling Load.
                    // Not sure why, this whole thing is just nasty.
                    // There must be a nicer .NET core version of:
                    //   var types = AppDomain.CurrentDomain.GetAssemblies()
                    //       .SelectMany(s => s.GetTypes())
                    //       .Where(p => type.IsAssignableFrom(p));
                }
            }

            _isPrimed = true;
        }
    }
}