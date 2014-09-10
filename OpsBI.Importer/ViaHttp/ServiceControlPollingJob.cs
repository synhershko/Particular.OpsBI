using NElasticsearch;
using Quartz;

namespace OpsBI.Importer.ViaHttp
{
    public abstract class ServiceControlPollingJob : IJob
    {
        public ServiceControlHttpConnection ServiceControlClient { get; set; }
        public ElasticsearchRestClient ElasticsearchClient { get; set; }

        public void Execute(IJobExecutionContext context)
        {
            var serviceControl = ServiceControlClient ?? (ServiceControlHttpConnection)context.Scheduler.Context.Get("servicecontrol");
            var elasticsearchClient = ElasticsearchClient ?? (ElasticsearchRestClient)context.Scheduler.Context.Get("elasticsearch");
            Execute(serviceControl, elasticsearchClient);
        }

        public abstract void Execute(ServiceControlHttpConnection serviceControl,
            ElasticsearchRestClient elasticsearchClient);
    }
}
