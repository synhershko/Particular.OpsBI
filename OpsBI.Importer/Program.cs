using System;
using NElasticsearch;
using OpsBI.Importer.ViaHttp;
using Quartz;

namespace OpsBI.Importer
{
    class Program
    {
        private string ServiceControlUrl = "";

        static void Main(string[] args)
        {
            var reporter = new ReportToElasticsearch(ServiceControlUrl, new ElasticsearchRestClient("http://localhost:9200/"));

            try
            {
                reporter.Start();

                Console.ReadKey();

                reporter.Stop();
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se.Message);
            }
        }
    }
}
