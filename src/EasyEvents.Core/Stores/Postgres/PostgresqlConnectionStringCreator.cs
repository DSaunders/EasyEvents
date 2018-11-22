using System;

namespace EasyEvents.Core.Stores.Postgres
{
    public static class PostgresqlConnectionStringCreator
    {
        public static string FromUrl(string url, bool requireSsl = false)
        {
            Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl);
            
            var result = 
                $"User ID={parsedUrl.UserInfo.Split(':')[0]};Password={parsedUrl.UserInfo.Split(':')[1]};Host={parsedUrl.Host};Port={parsedUrl.Port};Database={parsedUrl.LocalPath.Substring(1)};Pooling=true;";

            if (requireSsl)
                result += "Use SSL Stream=True;SSL Mode=Require;TrustServerCertificate=True;";

            return result;
        }
    }
}