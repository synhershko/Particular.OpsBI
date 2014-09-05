using System.IO;
using System.Text;
using Nancy;
using Nancy.Conventions;

namespace OpsBI.Tests.Infrastructure
{
    public class NancyBootstrapper : DefaultNancyBootstrapper
    {
        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);

            conventions.StaticContentsConventions.Clear();
            conventions.StaticContentsConventions.Add((ctx, root) =>
            {
                var reqPath = ctx.Request.Path;

//                if (reqPath.Equals("/"))
//                {
//                    reqPath = "/index.html";
//                }

                var jsonBytes = Encoding.UTF8.GetBytes(File.ReadAllText(@"Z:\code\Particular\Particular.OpsBI\OpsBI.Tests\Data\FailedMessages.json"));
                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length)
                };
            });
        }
    }
}
