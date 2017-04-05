namespace EasyEvents.SampleWebApp
{
    using System.Text;
    using System.Threading.Tasks;
    using Core;
    using Events.AppEvents;
    using Nancy;

    public class SampleModule : NancyModule
    {
        public SampleModule(IEasyEvents easyEvents)
        {
            Get("/", x => Task.FromResult("Hello"));

            Get("/thing/{happened}", async (x, token) =>
            {
                await easyEvents.RaiseEventAsync(new ThingHappenedEvent((string) x.happened));
                return "Done";
            });

            Get("/log", (x) =>
            {
                var s = new StringBuilder();

                foreach (var message in Logger.Log)
                {
                    s.AppendLine(message + "<br/>");
                }
                return s.ToString();
            });
        }
    }
}