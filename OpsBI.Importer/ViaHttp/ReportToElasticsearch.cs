﻿using System;
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
        private readonly ElasticsearchRestClient _elasticsearchClient;

        public TimeSpan PollingPeriod { get; set; }
        private ServiceControlHttpConnection ServiceControl { get; set; }

        protected static readonly Func<DateTime, string> _resolveIndexName = t => "opsbi-" + t.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture);

        private readonly IScheduler _scheduler;

        public ReportToElasticsearch(string serviceControlUrl, ElasticsearchRestClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            PollingPeriod = TimeSpan.FromSeconds(10);
            ServiceControl = new ServiceControlHttpConnection(serviceControlUrl);
            
            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
            _scheduler.Context.Add("servicecontrol", ServiceControl);
            _scheduler.Context.Add("elasticsearch", elasticsearchClient);
        }

        public void Start()
        {
            var newTrigger = new Func<ITrigger>(() => TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(PollingPeriod).RepeatForever())
                .Build());

            _scheduler.ScheduleJob(JobBuilder.Create<EndpointPolling>().Build(), newTrigger());
            _scheduler.ScheduleJob(JobBuilder.Create<MessagePolling>().Build(), newTrigger());
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

                if (!bulkOperation.BulkOperationItems.Any())
                {
                    Console.WriteLine("No endpoints found");
                    return;
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

        class MessagePolling : ServiceControlPollingJob
        {
            internal static Message LastMessageSeen = new Message {TimeSent = DateTime.MinValue};

            public override void Execute(ServiceControlHttpConnection serviceControl, ElasticsearchRestClient elasticsearchClient)
            {
                var now = DateTime.UtcNow;
                var bulkOperation = BulkOperation.On(_resolveIndexName(now), "message");

                Message lastMessageSeenInThisRound = null;
                int currentPage, resultsCollected = 0;
                var pagedResults = serviceControl.GetAuditMessages(currentPage = 1);
                while (pagedResults.TotalCount > resultsCollected && pagedResults.Result.Count > 0)
                {
                    if (currentPage == 1)
                    {
                        lastMessageSeenInThisRound = new Message(pagedResults.Result[0]);
                    }

                    foreach (var message in pagedResults.Result)
                    {
                        var m = new Message(message);
                        if (m.TimeSent <= LastMessageSeen.TimeSent && m.MessageId.Equals(LastMessageSeen.MessageId))
                            goto postMesssages;

                        var jobject = JObject.FromObject(m);
                        jobject["@timestamp"] = m.TimeSent; // Stamp with datetime in the format Kibana expects
                        bulkOperation.Index(jobject.ToString(Formatting.None));
                        resultsCollected++;
                    }

                    pagedResults = serviceControl.GetAuditMessages(++currentPage);
                }

postMesssages:

                if (!bulkOperation.BulkOperationItems.Any())
                {
                    Console.WriteLine("No messages found");
                    return;
                }

                if (lastMessageSeenInThisRound != null)
                {
                    LastMessageSeen = lastMessageSeenInThisRound;
                }

                Console.WriteLine("Posting {0} messages", bulkOperation.BulkOperationItems.Count());

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
