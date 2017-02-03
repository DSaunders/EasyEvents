namespace EventDb.SampleWebApp
{
    using System.Text;
    using Events.AppEvents;
    using EventDb.Core;
    using Nancy;
    
    public class SampleModule : NancyModule
    {
        public SampleModule(IEventDb eventDb)
        {
            Get["/"] = _ => "Hello";

            Get["/thing/{happened}", true] = async (_,c) =>
            {
                await eventDb.RaiseEventAsync(new ThingHappenedEvent((string) _.happened));
                return "Done";
            };

            Get["/log"] = _ =>
            {
                var s = new StringBuilder();
                
                foreach (var message in Logger.Log)
                {
                    s.AppendLine(message + "<br/>");
                }
                return  s.ToString();
            };
        }
    }
}