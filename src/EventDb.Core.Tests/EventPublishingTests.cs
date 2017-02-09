namespace EventDb.Core.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClientInterfaces;
    using Configuration;
    using Exceptions;
    using Shouldly;
    using Stores;
    using TestEvents;
    using Xunit;

    public class EventPublishingTests
    {
        private readonly SimpleTextEventHandler _simpleTextEventHandler;
        private readonly IEventDb _eventDb;
        private readonly List<SimpleTextEvent> _eventList;

        public EventPublishingTests()
        {
            _eventList = new List<SimpleTextEvent>();
            _simpleTextEventHandler = new SimpleTextEventHandler(_eventList);

            _eventDb = new EventDb();

            _eventDb.Configure(new EventDbConfiguration
            {
                HandlerFactory = type => _simpleTextEventHandler,
                EventStore = new InMemoryEventStore()
            });
        }

        [Fact]
        public void Event_Calls_Handlers()
        {
            // Given
            var testEvent = new SimpleTextEvent("test");

            // When
            _eventDb.RaiseEventAsync(testEvent);

            // Then
            _eventList.Count.ShouldBe(1);
            _eventList[0].ShouldBe(testEvent);
        }

        [Fact]
        public void Replays_All_Events_In_Order()
        {
            // Given
            _eventDb.RaiseEventAsync(new SimpleTextEvent("event 1"));
            _eventDb.RaiseEventAsync(new SimpleTextEvent("event 2"));
            _eventDb.RaiseEventAsync(new SimpleTextEvent("event 3"));
            _eventList.Clear();

            // When
            _eventDb.ReplayAllEventsAsync();

            // Then
            _eventList[0].SomeTestValue.ShouldBe("event 1");
            _eventList[1].SomeTestValue.ShouldBe("event 2");
            _eventList[2].SomeTestValue.ShouldBe("event 3");
        }

        [Fact]
        public async Task Throws_If_HandlerFactory_Returns_Type_That_Does_Not_Implement_Handler_Interface()
        {
            // Given
            _eventDb.Configure(new EventDbConfiguration
            {
                HandlerFactory = type => new object(),
                EventStore = new InMemoryEventStore()
            });

            // When
            var ex =
                await Record.ExceptionAsync(() => _eventDb.RaiseEventAsync(new SimpleTextEvent("this should explode")));

            // Then
            ex.ShouldNotBeNull();
            ex.ShouldBeOfType<EventHandlerException>();
            ex.Message.ShouldBe(
                $"Cannot handle {nameof(SimpleTextEvent)}. Handler returned from Factory does not implement {nameof(IEventHandler<IEvent>)}<{nameof(SimpleTextEvent)}>");
        }

        [Fact]
        public async Task Throws_If_HandlerFactory_Returns_Handler_That_Does_Not_Handle_This_Event_Type()
        {
            // Given
            _eventDb.Configure(new EventDbConfiguration
            {
                HandlerFactory = type => new NullEventHandler(),
                EventStore = new InMemoryEventStore()
            });

            // When
            var ex =
                await Record.ExceptionAsync(() => _eventDb.RaiseEventAsync(new SimpleTextEvent("this should explode")));

            // Then
            ex.ShouldNotBeNull();
            ex.ShouldBeOfType<EventHandlerException>();
            ex.Message.ShouldBe(
                $"Cannot handle {nameof(SimpleTextEvent)}. Handler returned from Factory does not implement {nameof(IEventHandler<IEvent>)}<{nameof(SimpleTextEvent)}>");
        }

        [Fact]
        public void Adds_Processors_To_Event_Streams()
        {
            // Given
            _eventDb.AddProcessorForStream("TestStream", async (s, e) =>
            {
                var typedEvent = e as SimpleTextEvent;

                if (typedEvent != null && typedEvent.SomeTestValue == "First event")
                    await _eventDb.RaiseEventAsync(new SimpleTextEvent(typedEvent.SomeTestValue + " re-raised"));

            });

            // When
            _eventDb.RaiseEventAsync(new SimpleTextEvent("First event"));

            // Then
            _eventList.Count.ShouldBe(2);
            _eventList[1].SomeTestValue.ShouldBe("First event re-raised");
        }

        [Fact]
        public void Does_Not_Run_Processors_On_Incorrect_Strean()
        {
            // Given
            _eventDb.AddProcessorForStream("SomeOtherStream", async (s, e) =>
            {
                await _eventDb.RaiseEventAsync(new SimpleTextEvent("This shouldn't happen"));
            });

            // When
            _eventDb.RaiseEventAsync(new SimpleTextEvent("First event"));

            // Then
            _eventList.Count.ShouldBe(1);
        }

        [Fact]
        public void Allows_Processors_To_Store_State_For_A_Stream()
        {
            // Given
            _eventDb.AddProcessorForStream("TestStream", async (s, e) =>
            {
                s["count"] = s.ContainsKey("count")
                    ? (int) s["count"] + 1
                    : 1;

                if ((int) s["count"] == 3)
                    await _eventDb.RaiseEventAsync(new SimpleTextEvent("Third event fired"));
            });

            // When
            _eventDb.RaiseEventAsync(new SimpleTextEvent("Event 1"));
            _eventDb.RaiseEventAsync(new SimpleTextEvent("Event 2"));
            _eventDb.RaiseEventAsync(new SimpleTextEvent("Event 3"));
            _eventDb.RaiseEventAsync(new SimpleTextEvent("Event 4"));

            // Then
            _eventList.Count.ShouldBe(5);
            _eventList[0].SomeTestValue.ShouldBe("Event 1");
            _eventList[1].SomeTestValue.ShouldBe("Event 2");
            _eventList[2].SomeTestValue.ShouldBe("Event 3");
            _eventList[3].SomeTestValue.ShouldBe("Third event fired");
            _eventList[4].SomeTestValue.ShouldBe("Event 4");
        }

        [Fact]
        public void Handlers_Raise_Events_During_Normal_Operation()
        {
            // Given
            var store = new TestEventStore();
            _eventDb.Configure(new EventDbConfiguration
            {
                EventStore = store,
                HandlerFactory = type =>
                {
                    if (type == typeof(IEventHandler<RaisesAnotherEvent>))
                        return new RaisesAnotherEventHandler(_eventDb);
                    return new NullEventHandler();
                }
            });

            // When
            _eventDb.RaiseEventAsync(new RaisesAnotherEvent()).Wait();

            // Then
            store.Events[0].ShouldBeOfType<RaisesAnotherEvent>();
            store.Events[1].ShouldBeOfType<NullEvent>();
        }

        [Fact]
        public void Does_Not_Raise_Events_Again_When_Replaying_Events()
        {
            // Given
            var store = new TestEventStore();
            _eventDb.Configure(new EventDbConfiguration
            {
                EventStore = store,
                HandlerFactory = type =>
                {
                    if (type == typeof(IEventHandler<RaisesAnotherEvent>))
                        return new RaisesAnotherEventHandler(_eventDb);
                    return new NullEventHandler();
                }
            });
            _eventDb.RaiseEventAsync(new RaisesAnotherEvent()).Wait();
            
            // When
            _eventDb.ReplayAllEventsAsync().Wait();

            // Then
            store.Events.Count.ShouldBe(2);
            store.Events[0].ShouldBeOfType<RaisesAnotherEvent>();
            store.Events[1].ShouldBeOfType<NullEvent>();
        }
    }
}
