using EasyEvents.Core.Stores.Postgres;
using Shouldly;
using Xunit;

namespace EasyEvents.Core.IntegrationTests
{
    public class PostgresqlConnectionStringCreatorTests
    {
        [Fact]
        public void Converts_From_Url()
        {
            var databaseUrl = "postgres://username:password@blah.com:5432/dbname";

            var conStr = PostgresqlConnectionStringCreator.FromUrl(databaseUrl);

            conStr.ShouldBe("User ID=username;Password=password;Host=blah.com;Port=5432;Database=dbname;Pooling=true;");

        }
        
        [Fact]
        public void Appends_SSL_Section_If_Requires_SSL()
        {
            var databaseUrl = "postgres://username:password@blah.com:5432/dbname";

            var conStr = PostgresqlConnectionStringCreator.FromUrl(databaseUrl, true);

            conStr.ShouldBe("User ID=username;Password=password;Host=blah.com;Port=5432;Database=dbname;Pooling=true;Use SSL Stream=True;SSL Mode=Require;TrustServerCertificate=True;");

        }
    }
}