namespace EasyEvents.Core.Stores
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using ClientInterfaces;
    using Newtonsoft.Json;

    public class SqlEventStore : IStore
    {
        private readonly string _connectionString;
        private bool _tableExists;

        public Func<IEvent, Task> EventHandler { get; set; }
        
        public SqlEventStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RaiseEventAsync(IEvent @event)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                await EnsureStreamTableExists(conn).ConfigureAwait(false);

                var command = new SqlCommand(
                    "insert into events (streamName, eventName, payload, created) values (@0,@1,@2,@3)",
                    conn);

                command.Parameters.AddWithValue("@0", @event.Stream);
                command.Parameters.AddWithValue("@1", @event.GetType().FullName);
                command.Parameters.AddWithValue("@2", JsonConvert.SerializeObject(@event));
                command.Parameters.AddWithValue("@3", DateTime.UtcNow);

                await command.ExecuteScalarAsync().ConfigureAwait(false);
            }

            await EventHandler.Invoke(@event).ConfigureAwait(false);
        }

        public async Task ReplayAllEvents()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // We're going to have to get all tables here, or create one table that 
                // hold a list of the stream tables 
                // As for how to get them, either union them (slow?) or get them all into memory and sort there?

                await EnsureStreamTableExists(conn).ConfigureAwait(false);

                var command = new SqlCommand("Select * from events order by created asc", conn);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var eventType = (string)reader["eventName"];
                        var type = GetEventType(eventType);
                        if (type == null)
                            continue;

                        var e = (IEvent)JsonConvert.DeserializeObject((string)reader["payload"], type);
                        await EventHandler.Invoke(e).ConfigureAwait(false);
                    }
                }
            }
        }

        private Type GetEventType(string eventName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var eventType = assembly.GetType(eventName);
                if (eventType != null)
                    return eventType;
            }

            return null;
        }
        
        private async Task EnsureStreamTableExists(SqlConnection conn, string tableNames)
        {
            if (_tableExists)
                return;

            var command = new SqlCommand(
                "If not exists (select name from sysobjects where name = 'events') " +
                "    CREATE TABLE events(streamName nvarchar(max), eventName nvarchar(max), payload nvarchar(max), created datetime2)",
                conn);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            _tableExists = true;
        }
    }
}