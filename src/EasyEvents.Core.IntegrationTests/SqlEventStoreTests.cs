namespace EasyEvents.Core.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using ClientInterfaces;
    using Newtonsoft.Json;
    using Shouldly;
    using Stores;
    using Stores.Sql;
    using Xunit;

    public class SqlEventStoreTests
    {
        private readonly string _conStr = "server=.;database=events.tests;Integrated Security=true;";

        [Fact]
        public async Task Run_Test_In_Order()
        {
            // Ensures tests don't run in parallel, this is only for dev testing anyway
            await Raises_And_Replays_Single_Event();
            await Replays_All_Events_In_Order();

            // Requires an API not yet in .NET Core
            //await Subscribes_To_Events_From_Outside_Application();
        }


        public async Task Raises_And_Replays_Single_Event()
        {
            var store = new SqlServerEventStore(_conStr);
            DeleteEventsTable();

            IEvent eventReceived = null;
            store.EventHandler = e =>
            {
                eventReceived = e;
                return Task.CompletedTask;
            };

            await store.RaiseEventAsync(new TestEvent("Hello, World!"));

            eventReceived.Stream.ShouldBe("test-stream");
            ((TestEvent) eventReceived).TestValue.ShouldBe("Hello, World!");
        }


        public async Task Replays_All_Events_In_Order()
        {
            var store = new SqlServerEventStore(_conStr);
            DeleteEventsTable();

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

        public async Task Subscribes_To_Events_From_Outside_Application()
        {
            var store = new SqlServerEventStore(_conStr);
            DeleteEventsTable();

            IEvent eventReceived = null;
            store.EventHandler = e =>
            {
                eventReceived = e;
                return Task.CompletedTask;
            };

            // Manually add an event
            using (var conn = new SqlConnection(_conStr))
            {
                conn.Open();

                var createTable = new SqlCommand(
               "If not exists (select name from sysobjects where name = 'events') " +
               "    CREATE TABLE events(streamName nvarchar(max), eventName nvarchar(max), payload nvarchar(max), created datetime2)",
               conn);

                await createTable.ExecuteNonQueryAsync().ConfigureAwait(false);

                var command = new SqlCommand(
                    "insert into events (streamName, eventName, payload, created) values (@0,@1,@2,@3)",
                    conn);

                command.Parameters.AddWithValue("@0", "other-app-stream");
                command.Parameters.AddWithValue("@1", typeof(TestEvent).AssemblyQualifiedName);
                command.Parameters.AddWithValue("@2", JsonConvert.SerializeObject(new TestEvent("This is a test")));
                command.Parameters.AddWithValue("@3", DateTime.Now);

                await command.ExecuteScalarAsync().ConfigureAwait(false);
            }

            // Check we received it
            eventReceived.Stream.ShouldBe("other-app-stream");
            ((TestEvent)eventReceived).TestValue.ShouldBe("Hello, World!");
        }


        private void DeleteEventsTable()
        {
            using (var conn = new SqlConnection(_conStr))
            {
                conn.Open();
                var command = new SqlCommand(
                    "If exists (select name from sysobjects where name = 'events') drop table events",
                    conn);

                command.ExecuteNonQuery();
            }
        }
    }
}
