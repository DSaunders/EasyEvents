﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;
using EasyEvents.Core.Stores.FileSystem;
using Shouldly;
using Xunit;

namespace EasyEvents.Core.IntegrationTests
{
    public class FileSystemEventStoreTests
    {
        private const string EventPath = "/integration-test-events";

        [Fact]
        public async Task Run_Test_In_Order()
        {
            await Does_Not_Throw_Replaying_No_Events();
            await Raises_And_Replays_Single_Event();
            await Replays_All_Events_In_Order();
            await Aggregates_Multiple_Streams_In_Order();
        }

        public async Task Does_Not_Throw_Replaying_No_Events()
        {
            var store = new FileSystemEventStore(EventPath);
            DeleteEvents();
            await store.ReplayAllEvents();
        }

        public async Task Raises_And_Replays_Single_Event()
        {
            var store = new FileSystemEventStore(EventPath);
            DeleteEvents();

            IEvent eventReceived = null;
            store.EventHandler = e =>
            {
                eventReceived = e;
                return Task.CompletedTask;
            };

            await store.RaiseEventAsync(new TestEvent("Hello, World!"));

            eventReceived.Stream.ShouldBe("test-stream");
            ((TestEvent)eventReceived).TestValue.ShouldBe("Hello, World!");
        }

        public async Task Replays_All_Events_In_Order()
        {
            var store = new FileSystemEventStore(EventPath);
            DeleteEvents();

            var eventsReceived = new List<IEvent>();
            store.EventHandler = e =>
            {
                eventsReceived.Add(e);
                return Task.CompletedTask;
            };

            // Raise some events
            await store.RaiseEventAsync(new TestEvent("Hello, World!"));
            await store.RaiseEventAsync(new TestEvent("Hello, Bob!"));
            await store.RaiseEventAsync(new TestEvent("Hello, Dave!"));

            // Pretend we haven't seen them yet
            eventsReceived.Clear();

            // Replay and check they are correct
            await store.ReplayAllEvents();

            eventsReceived[0].Stream.ShouldBe("test-stream");
            ((TestEvent)eventsReceived[0]).TestValue.ShouldBe("Hello, World!");
            eventsReceived[1].Stream.ShouldBe("test-stream");
            ((TestEvent)eventsReceived[1]).TestValue.ShouldBe("Hello, Bob!");
            eventsReceived[2].Stream.ShouldBe("test-stream");
            ((TestEvent)eventsReceived[2]).TestValue.ShouldBe("Hello, Dave!");
        }

        public async Task Aggregates_Multiple_Streams_In_Order()
        {
            var store = new FileSystemEventStore(EventPath);
            DeleteEvents();

            var eventsReceived = new List<IEvent>();
            store.EventHandler = e =>
            {
                eventsReceived.Add(e);
                return Task.CompletedTask;
            };

            // Raise some events
            await store.RaiseEventAsync(new TestEvent("Hello, World!"));
            await store.RaiseEventAsync(new AnotherTestEvent("Hello, Bob!"));
            await store.RaiseEventAsync(new TestEvent("Hello, Dave!"));

            // Pretend we haven't seen them yet
            eventsReceived.Clear();

            // Replay and check they are correct
            await store.ReplayAllEvents();

            eventsReceived[0].Stream.ShouldBe("test-stream");
            ((TestEvent)eventsReceived[0]).TestValue.ShouldBe("Hello, World!");
            eventsReceived[1].Stream.ShouldBe("another-test-stream");
            ((AnotherTestEvent)eventsReceived[1]).TestValue.ShouldBe("Hello, Bob!");
            eventsReceived[2].Stream.ShouldBe("test-stream");
            ((TestEvent)eventsReceived[2]).TestValue.ShouldBe("Hello, Dave!");
        }


        private void DeleteEvents()
        {
            if (Directory.Exists(EventPath))
                Directory.Delete(EventPath, true);
        }
    }
}
