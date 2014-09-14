using System;
using System.Collections.Generic;
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

        private readonly List<PersistentPollingExecuter> persistentPollers = new List<PersistentPollingExecuter>();

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

            var indexName = _resolveIndexName(DateTime.Now);
            if (!elasticsearchClient.IndexExists(indexName))
            {
                elasticsearchClient.CreateIndex(indexName);
                elasticsearchClient.PutMappingFor<Message>(indexName);
                elasticsearchClient.PutMappingFor<EndpointHearbeatStatus>(indexName);
            }
        }

        public void Start()
        {
            // Get the timestamp for the last known audit message
            var messages = _elasticsearchClient.Search<Message>(
                new
                {
                    query = new { match_all = new {}},
                    filter = new { term = new {IsFailed = false}},
                    sort = new object[]{new {TimeSent = "desc"}},
                    from = 0, size = 1,
                },
                "opsbi-*", "message");
            if (messages.hits.hits != null && messages.hits.hits.Count > 0)
            {
                MessagePolling.LastMessageSeen = messages.hits.hits[0]._source;
            }

            // Get the timestamp for the last known failed message
            messages = _elasticsearchClient.Search<Message>(
                new
                {
                    query = new { match_all = new { } },
                    filter = new { term = new { IsFailed = true } },
                    sort = new object[] { new { TimeSent = "desc" } },
                    from = 0,
                    size = 1,
                },
                "opsbi-*", "message");
            if (messages.hits.hits != null && messages.hits.hits.Count > 0)
            {
                FailedMessagePolling.LastMessageSeen = messages.hits.hits[0]._source;
            }

            var newTrigger = new Func<ITrigger>(() => TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(PollingPeriod).RepeatForever())
                .Build());

            persistentPollers.Add(new PersistentPollingExecuter(new MessagePolling(), ServiceControl, _elasticsearchClient).Start());
            persistentPollers.Add(new PersistentPollingExecuter(new FailedMessagePolling(), ServiceControl, _elasticsearchClient).Start());

            _scheduler.ScheduleJob(JobBuilder.Create<EndpointPolling>().Build(), newTrigger());
            _scheduler.ScheduleJob(JobBuilder.Create<CustomChecksPolling>().Build(), newTrigger());
            _scheduler.Start();
        }

        public void Stop()
        {
            _scheduler.Shutdown(true);
            _scheduler.Clear();
            persistentPollers.ForEach(executer => executer.Shutdown());
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

        public class MessagePolling : ServiceControlPollingJob
        {
            internal static Message LastMessageSeen = new Message {TimeSent = DateTime.MinValue};

            public override void Execute(ServiceControlHttpConnection serviceControl, ElasticsearchRestClient elasticsearchClient)
            {
                var now = DateTime.UtcNow;
                var threshold = now.AddDays(-1);

                // TODO index name may change within a bulk
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
                        if (message.TimeSent <= LastMessageSeen.TimeSent && message.Id.Equals(LastMessageSeen.MessageId))
                            goto postMesssages;

                        if (message.TimeSent <= threshold)
                        {
                            Console.WriteLine("Message polling: Met threshold, stopping consumption");
                            goto postMesssages;
                        }

                        var m = new Message(message);
                        var jobject = JObject.FromObject(m);
                        jobject["@timestamp"] = m.TimeSent; // Stamp with datetime in the format Kibana expects
                        bulkOperation.Index(jobject.ToString(Formatting.None));
                        resultsCollected++;
                    }

                    // TODO this is to prevent slow initializations, but during normal operation
                    // TODO this can still happen, and will need to autotune polling period
                    if (bulkOperation.BulkOperationItems.Count >= 500)
                        break;

                    pagedResults = serviceControl.GetAuditMessages(++currentPage);
                }

postMesssages:

                if (!bulkOperation.BulkOperationItems.Any())
                {
                    Console.WriteLine("Message polling: No messages found, last timestamp seen: {0}", LastMessageSeen.TimeSent);
                    return;
                }

                Console.WriteLine("Message polling: Posting {0} messages, latest timestamp seen: {1}", bulkOperation.BulkOperationItems.Count(), lastMessageSeenInThisRound.TimeSent);

                try
                {
                    elasticsearchClient.Bulk(bulkOperation);
                    LastMessageSeen = lastMessageSeenInThisRound;
                }
                catch (ElasticsearchException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public class FailedMessagePolling : ServiceControlPollingJob
        {
            internal static Message LastMessageSeen = new Message { TimeSent = DateTime.MinValue };

            public override void Execute(ServiceControlHttpConnection serviceControl, ElasticsearchRestClient elasticsearchClient)
            {
                var now = DateTime.UtcNow;
                var threshold = now.AddDays(-1);

                // TODO index name may change within a bulk
                var bulkOperation = BulkOperation.On(_resolveIndexName(now), "message");

                Message lastMessageSeenInThisRound = null;
                int currentPage, resultsCollected = 0;

                var pagedResults = serviceControl.GetErrorMessages(currentPage = 1);
                while (pagedResults.TotalCount > resultsCollected && pagedResults.Result.Count > 0)
                {
                    if (currentPage == 1)
                    {
                        lastMessageSeenInThisRound = new Message(pagedResults.Result[0]);
                    }

                    foreach (var message in pagedResults.Result)
                    {
                        if (message.TimeSent <= LastMessageSeen.TimeSent && message.Id.Equals(LastMessageSeen.MessageId))
                            goto postMesssages;

                        if (message.TimeSent <= threshold)
                        {
                            Console.WriteLine("ErrorMessage polling: Met threshold, stopping consumption");
                            goto postMesssages;
                        }

                        var m = new Message(message);
                        var jobject = JObject.FromObject(m);
                        jobject["@timestamp"] = m.TimeSent; // Stamp with datetime in the format Kibana expects
                        bulkOperation.Index(jobject.ToString(Formatting.None));
                        resultsCollected++;
                    }

                    // TODO this is to prevent slow initializations, but during normal operation
                    // TODO this can still happen, and will need to autotune polling period
                    if (bulkOperation.BulkOperationItems.Count >= 500)
                        break;

                    pagedResults = serviceControl.GetErrorMessages(++currentPage);
                }

postMesssages:
                if (!bulkOperation.BulkOperationItems.Any())
                {
                    Console.WriteLine("ErrorMessage polling: No messages found, last timestamp seen: {0}", LastMessageSeen.TimeSent);
                    return;
                }

                Console.WriteLine("ErrorMessage polling: Posting {0} messages, latest timestamp seen: {1}", bulkOperation.BulkOperationItems.Count(), lastMessageSeenInThisRound.TimeSent);

                try
                {
                    elasticsearchClient.Bulk(bulkOperation);
                    LastMessageSeen = lastMessageSeenInThisRound;
                }
                catch (ElasticsearchException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        class CustomChecksPolling : ServiceControlPollingJob
        {
            public override void Execute(ServiceControlHttpConnection serviceControl, ElasticsearchRestClient elasticsearchClient)
            {
                var now = DateTime.UtcNow;
                var bulkOperation = BulkOperation.On(_resolveIndexName(now), "customcheck");

                var customChecks = serviceControl.GetCustomChecks();
                foreach (var c in customChecks)
                {
                    var jobject = JObject.FromObject(new CustomCheckStatus(c));
                    jobject["@timestamp"] = now; // Stamp with datetime in the format Kibana expects
                    // TODO try figuring out from c.ReportedAt if the check report is stale
                    bulkOperation.Index(jobject.ToString(Formatting.None));
                }

                if (!bulkOperation.BulkOperationItems.Any())
                {
                    Console.WriteLine("No custom checks found");
                    return;
                }

                Console.WriteLine("Reporting on {0} custom checks", bulkOperation.BulkOperationItems.Count());

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
    }
}
