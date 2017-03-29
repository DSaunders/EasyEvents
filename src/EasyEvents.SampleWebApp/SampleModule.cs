namespace EasyEvents.SampleWebApp
{
    using System.Text;
    using Core;
    using Events.AppEvents;
    using Nancy;

    public class SampleModule : NancyModule
    {
        public SampleModule(IEasyEvents easyEvents)
        {
            Get["/"] = _ => "Hello";

            Get["/thing/{happened}", true] = async (_,c) =>
            {
                await easyEvents.RaiseEventAsync(new ThingHappenedEvent((string) _.happened));
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