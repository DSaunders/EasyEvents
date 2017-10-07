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

        private EasyEventsConfiguration _config;

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

            PopulateDateTimeProperty(@event);

            await _config.Store.RaiseEventAsync(@event).ConfigureAwait(false);
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


        private async Task DispatchEvent(IEvent @event)
        {
            await DisptchEventToHandler(@event).ConfigureAwait(false);
            await RunProcessorsForEvent(@event).ConfigureAwait(false);
        }

        private async Task DisptchEventToHandler(IEvent @event)
        {
            var targetHandlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());

            var handler = _config.HandlerFactory(targetHandlerType);
            if (handler == null)
                return;

            if (!CanHandleEvent(handler, @event))
            {
                var eventTypeName = @event.GetType().Name;
                throw new EventHandlerException($"Cannot handle {eventTypeName}. " +
                                                $"Handler returned from Factory does not implement {nameof(IEventHandler<IEvent>)}<{eventTypeName}>");
            }

            var method = targetHandlerType.GetMethod(nameof(IEventHandler<IEvent>.HandleEventAsync));
            await ((Task) method.Invoke(handler, new[] {@event})).ConfigureAwait(false);
        }

        private async Task RunProcessorsForEvent(IEvent @event)
        {
            foreach (var processor in _processors.GetProcessorsForStream(@event.Stream))
                await processor.Invoke(_streamState.GetStreamState(@event.Stream), @event).ConfigureAwait(false);
        }

        private bool CanHandleEvent(object handlerCandidate, IEvent @event)
        {
            return handlerCandidate.GetType().GetInterfaces().Any(x =>
                x.GetTypeInfo().IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEventHandler<>) &&
                x.GenericTypeArguments.Count() == 1 &&
                x.GenericTypeArguments[0] == @event.GetType());
        }

        private void PopulateDateTimeProperty(IEvent @event)
        {
            var property = @event.GetType().GetProperty("DateTime");
            if (property != null &&
            property.PropertyType == typeof(DateTime) &&
            property.CanWrite)
            {
                property.SetValue(@event, DateAbstraction.UtcNow);
            }
        }

    }
}