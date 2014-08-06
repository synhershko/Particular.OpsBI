using System;
using System.Globalization;
using System.Linq;
using NElasticsearch;
using NElasticsearch.Commands;
using NElasticsearch.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpsBI.Importer.Models;
using Quartz;
using Quartz.Impl;

namespace OpsBI.Importer.ViaHttp
{
    public class ReportToElasticsearch
    {
        public TimeSpan PollingPeriod { get; set; }
        private ServiceControlHttpConnection ServiceControl { get; set; }

        protected static readonly Func<DateTime, string> _resolveIndexName = t => t.ToString("opsbi-yyyy.MM.dd", CultureInfo.InvariantCulture);

        private readonly IScheduler _scheduler;

        public ReportToElasticsearch(string serviceControlUrl, ElasticsearchRestClient elasticsearchClient = null)
        {
            PollingPeriod = TimeSpan.FromSeconds(10);
            ServiceControl = new ServiceControlHttpConnection(serviceControlUrl);
            
            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
            _scheduler.Context.Add("servicecontrol", ServiceControl);
            _scheduler.Context.Add("elasticsearch", elasticsearchClient);
        }

        public void Start()
        {
            var trigger = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(PollingPeriod).RepeatForever())
                .Build();

            _scheduler.ScheduleJob(JobBuilder.Create<EndpointPolling>().Build(), trigger);
            _scheduler.Start();
        }

        public void Stop()
        {
            _scheduler.Shutdown(true);
            _scheduler.Clear();
        }

        class EndpointPolling : ServiceControlPollingJob
        {
            public override void Execute(ServiceControlHttpConnection serviceControl, ElasticsearchRestClient elasticsearchClient)
            {
                var now = DateTime.UtcNow;
                var bulkOperation = BulkOperation.On(_resolveIndexName(now), "endpoint");

                var endpoints = serviceControl.GetEndpoints();
                foreach (var endpoint in endpoints)
                {
                    var jobject = JObject.FromObject(new EndpointHearbeatStatus(endpoint));
                    jobject["@timestamp"] = now; // Stamp with datetime in the format Kibana expects
                    bulkOperation.Index(jobject.ToString(Formatting.None));
                }
                Console.WriteLine("Reporting on {0} endpoints", bulkOperation.BulkOperationItems.Count());

                try
                {
                    elasticsearchClient.Bulk(bulkOperation);
                }
                catch (ElasticsearchException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        abstract class ServiceControlPollingJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                var serviceControl = (ServiceControlHttpConnection) context.Scheduler.Context.Get("servicecontrol");
                var elasticsearchClient = (ElasticsearchRestClient)context.Scheduler.Context.Get("elasticsearch");
                Execute(serviceControl, elasticsearchClient);
            }

            public abstract void Execute(ServiceControlHttpConnection serviceControl,
                ElasticsearchRestClient elasticsearchClient);
        }
    }
}
