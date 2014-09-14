using System.IO;
using System.Reflection;
using NElasticsearch;
using NElasticsearch.Commands;
using OpsBI.Dashboards;

namespace OpsBI.Dashboards
{
    class Program
    {
        static string KibanaIndexName = "kibana-int";
        static void Main(string[] args)
        {
            bool haveDashboards = false;
            var elasticsearchClient = new ElasticsearchRestClient("http://localhost:9200");
            var dashboard1 = elasticsearchClient.Get<Dashboard>("OpsBI.Messages", "dashboard", KibanaIndexName);
            if (dashboard1 != null && dashboard1.found)
            {
                File.WriteAllText(@"z:\OpsBI.Messages.json", dashboard1._source.dashboard);
                haveDashboards = true;
            }

            var dashboard2 = elasticsearchClient.Get<Dashboard>("OpsBI.Endpoints", "dashboard", KibanaIndexName);
            if (dashboard2 != null && dashboard2.found)
            {
                File.WriteAllText(@"z:\OpsBI.Endpoints.json", dashboard2._source.dashboard);
                haveDashboards = true;
            }

            var dashboard3 = elasticsearchClient.Get<Dashboard>("OpsBI.CustomChecks", "dashboard", KibanaIndexName);
            if (dashboard3 != null && dashboard3.found)
            {
                File.WriteAllText(@"z:\OpsBI.CustomChecks.json", dashboard3._source.dashboard);
                haveDashboards = true;
            }

            if (haveDashboards)
            {
                return;
            }

            var db = new Dashboard
            {
                title = "OpsBI.Endpoints",
                group = "guest",
                user = "guest",
                dashboard = GetEmbeddedJson(typeof (Program).Assembly, ".OpsBI.Endpoints.json"),
            };
            elasticsearchClient.Index(db, db.title, "dashboard", KibanaIndexName);

            db = new Dashboard
            {
                title = "OpsBI.Messages",
                group = "guest",
                user = "guest",
                dashboard = GetEmbeddedJson(typeof(Program).Assembly, ".OpsBI.Messages.json"),
            };
            elasticsearchClient.Index(db, db.title, "dashboard", KibanaIndexName);

            db = new Dashboard
            {
                title = "OpsBI.CustomChecks",
                group = "guest",
                user = "guest",
                dashboard = GetEmbeddedJson(typeof(Program).Assembly, ".OpsBI.CustomChecks.json"),
            };
            elasticsearchClient.Index(db, db.title, "dashboard", KibanaIndexName);
        }

        static string GetEmbeddedJson(Assembly assembly, string embeddedResourcePath)
        {
            if (!embeddedResourcePath.StartsWith(".")) embeddedResourcePath = "." + embeddedResourcePath;

            using (var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + embeddedResourcePath))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
