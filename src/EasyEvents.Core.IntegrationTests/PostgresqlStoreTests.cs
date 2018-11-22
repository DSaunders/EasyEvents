using EasyEvents.Core.Stores.Postgres;
using Npgsql;

namespace EasyEvents.Core.IntegrationTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClientInterfaces;
    using Shouldly;
    using Xunit;

    public class PostgresqlStoreTests
    {
        private readonly string _conStr =
            PostgresqlConnectionStringCreator.FromUrl("-- INSERT DATABASE URL HERE --", true);

        [Fact]
        public async Task Run_Test_In_Order()
        {
            // Ensures tests don't run in parallel, this is only for dev testing anyway
            await Raises_And_Replays_Single_Event();
            await Replays_All_Events_In_Order();
        }


        public async Task Raises_And_Replays_Single_Event()
        {
            var store = new PostgresqlEventStore(_conStr);
            DeleteEventsTable();

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
            var store = new PostgresqlEventStore(_conStr);
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

        private void DeleteEventsTable()
        {
            using (var conn = new NpgsqlConnection(_conStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DROP TABLE IF EXISTS events;";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
