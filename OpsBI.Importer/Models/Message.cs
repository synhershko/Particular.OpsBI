using System;
using NElasticsearch.Mapping;
using OpsBI.Importer.ViaHttp.Models;

namespace OpsBI.Importer.Models
{
    [ElasticsearchType(Name = "message")]
    public class Message
    {
        public Message(StoredMessage message)
        {
            this.MessageId = message.Id;
            this.Type = message.MessageType;
            this.TimeSent = message.TimeSent;
            this.SendingEndpointAddress = message.SendingEndpoint.Address;
            this.ReceivingEndpointAddress = message.ReceivingEndpoint.Address;
            this.Status = message.Status.ToString();

            // TODO this.ServiceInsightUrl = ;

            var failedMessage = message as FailedMessage;
            if (failedMessage != null)
            {
                this.IsFailed = true;
                if (failedMessage.Exception != null)
                {
                    ExceptionMessage = failedMessage.Exception.Message;
                    ExceptionSource = failedMessage.Exception.Source;
                    ExceptionStackTrace = failedMessage.Exception.StackTrace;
                    ExceptionType = failedMessage.Exception.ExceptionType;
                }
            }
            else
            {
                this.DeliveryTime = (int)message.DeliveryTime.TotalMilliseconds;
                this.ProcessingTime = (int)message.ProcessingTime.TotalMilliseconds;
            }
        }

        public Message()
        {            
        }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string MessageId { get; set; }
        
        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Type { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string SendingEndpointAddress { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string ReceivingEndpointAddress { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.No)]
        public string PlatformUrl { get; set; }

        [ElasticsearchProperty]
        public bool IsFailed { get; set; }

        [ElasticsearchProperty]
        public DateTime TimeSent { get; set; }

//        [ElasticsearchProperty]
//        public DateTime ProcessedAt { get; set; }

        [ElasticsearchProperty]
        public int? DeliveryTime { get; set; }

        [ElasticsearchProperty]
        public int? ProcessingTime { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Status { get; set; }

        // Failed Message specific:

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string ExceptionType { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string ExceptionMessage { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string ExceptionSource { get; set; }

        [ElasticsearchProperty]
        public string ExceptionStackTrace { get; set; }
    }
}
