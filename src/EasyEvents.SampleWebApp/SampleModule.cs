using System.Text;
using System.Threading.Tasks;
using EasyEvents.Core;
using EasyEvents.SampleWebApp.Events.AppEvents;
using EasyEvents.SampleWebApp.Events.TestEvents;
using Nancy;

namespace EasyEvents.SampleWebApp
{
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

            Get("/view/{page}", async (x, token) =>
            {
                await easyEvents.RaiseEventAsync(new PageViewedEvent((string)x.page));
                return "Page view to " + x.page + " logged";
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