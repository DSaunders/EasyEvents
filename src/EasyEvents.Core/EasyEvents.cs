namespace EasyEvents.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ClientInterfaces;
    using Configuration;
    using System.Reflection;
    using Exceptions;

    public class EasyEvents : IEasyEvents
    {
        public bool IsReplayingEvents { get; private set; }

        private static EasyEventsConfiguration _config;

        private readonly Processors _processors;
        private readonly StreamState _streamState;

        public EasyEvents()
        {
            _processors = new Processors();
            _streamState = new StreamState();
        }

        public void Configure(EasyEventsConfiguration config)
        {
            _config = config;
            config.Store.EventHandler += DispatchEvent;
        }

        public async Task RaiseEventAsync(IEvent @event)
        {
            if (IsReplayingEvents)
                return;

            await _config.Store.RaiseEventAsync(@event).ConfigureAwait(false);

            foreach (var processor in _processors.GetProcessorsForStream(@event.Stream))
                    await processor.Invoke(_streamState.GetStreamState(@event.Stream), @event).ConfigureAwait(false);
        }

        public async Task ReplayAllEventsAsync()
        {
            IsReplayingEvents = true;
            await _config.Store.ReplayAllEvents().ConfigureAwait(false);
            IsReplayingEvents = false;
        }

        public void AddProcessorForStream(string streamName, Func<Dictionary<string, object>, object, Task> processor)
        {
            _processors.AddProcessorForStream(streamName, processor);
        }

        private Task DispatchEvent(IEvent @event)
        {
            var targetHandlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());

            var handler = _config.HandlerFactory(targetHandlerType);
            if (handler == null)
                return Task.FromResult(0);

            if (!CanHandleEvent(handler, @event))
            {
                var eventTypeName = @event.GetType().Name;
                throw new EventHandlerException($"Cannot handle {eventTypeName}. " +
                                    $"Handler returned from Factory does not implement {nameof(IEventHandler<IEvent>)}<{eventTypeName}>");
            }

            var method = targetHandlerType.GetMethod(nameof(IEventHandler<IEvent>.HandleEventAsync));
            return (Task)method.Invoke(handler, new []{@event});
        }

        private bool CanHandleEvent(object handlerCandidate, IEvent @event)
        {
            return handlerCandidate.GetType().GetInterfaces().Any(x =>
                x.GetTypeInfo().IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEventHandler<>) &&
                x.GenericTypeArguments.Count() == 1 &&
                x.GenericTypeArguments[0] == @event.GetType());
        }
    }
}