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
                // TODO: Is there a less coupled way to store/load the Type name with .NET Core?
                // We used to be able to do AppDomain.CurrentDomain.GetAssemblies() and iterate the
                // assemblies to find the type by it's FullName, but that doesn't exist now.
                command.Parameters.AddWithValue("@1", @event.GetType().AssemblyQualifiedName);
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
            return Type.GetType(eventName);
        }

        private async Task EnsureStreamTableExists(SqlConnection conn)
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