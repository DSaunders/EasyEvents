namespace EventDb.SampleWebApp
{
    using System.Collections.Generic;

    public static class Logger
    {
        public static readonly List<string> Log = new List<string>();
        
        public static void LogMessage(string message)
        {
            Log.Add(message);
        }
    }
}