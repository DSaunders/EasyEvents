using System;
using System.Threading.Tasks;
using EasyEvents.Core.ClientInterfaces;
using Newtonsoft.Json;
using Npgsql;

namespace EasyEvents.Core.Stores.Postgres
{
    public class PostgresqlEventStore : IStore
    {
        private readonly string _connectionString;
        private readonly EventTypeCache _eventTypeCache;
        private bool _tableExists;

        public Func<IEvent, Task> EventHandler { get; set; }

        public PostgresqlEventStore(string connectionString)
        {
            _connectionString = connectionString;
            _eventTypeCache = new EventTypeCache();
        }

        public async Task RaiseEventAsync(IEvent @event)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                await EnsureStreamTableExists(conn).ConfigureAwait(false);

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText =
                        "insert into events (streamName, eventName, payload, created) values (@0,@1,@2,@3)";

                    cmd.Parameters.AddWithValue("@0", @event.Stream);
                    cmd.Parameters.AddWithValue("@1", @event.GetType().Name);
                    cmd.Parameters.AddWithValue("@2", JsonConvert.SerializeObject(@event));
                    cmd.Parameters.AddWithValue("@3", DateTime.UtcNow);

                    await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                }
            }

            await EventHandler.Invoke(@event).ConfigureAwait(false);
        }

        public async Task ReplayAllEvents()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                await EnsureStreamTableExists(conn).ConfigureAwait(false);

                using (var cmd = new NpgsqlCommand("select * from events order by created asc", conn))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        var eventType = (string)reader["eventName"];
                        var type = _eventTypeCache.GetEventTypeFromName(eventType);
                        if (type == null)
                            continue;

                        var e = (IEvent)JsonConvert.DeserializeObject((string)reader["payload"], type);
                        await EventHandler.Invoke(e).ConfigureAwait(false);
                    }
            }

        }

        private async Task EnsureStreamTableExists(NpgsqlConnection conn)
        {
            if (_tableExists)
                return;

            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS  events (" +
                                  "streamName text, " +
                                  "eventName text, " +
                                  "payload text, " +
                                  "created timestamp" +
                                  ");";

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                _tableExists = true;

            }
        }
    }
}