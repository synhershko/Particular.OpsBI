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
            this.SendingEndpointAddress = message.SendingEndpoint.Address;
            this.ReceivingEndpointAddress = message.ReceivingEndpoint.Address;
            //this.ServiceInsightUrl = ;
            this.IsFailed = !(message.Status == MessageStatus.Successful || message.Status == MessageStatus.ResolvedSuccessfully);
            this.TimeSent = message.TimeSent;
            //this.ProcessedAt = 
            this.DeliveryTime = (int) message.DeliveryTime.TotalMilliseconds;
            this.ProcessingTime = (int) message.ProcessingTime.TotalMilliseconds;
            this.Status = message.Status.ToString();
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

        [ElasticsearchProperty]
        public DateTime ProcessedAt { get; set; }

        [ElasticsearchProperty]
        public int DeliveryTime { get; set; }

        [ElasticsearchProperty]
        public int ProcessingTime { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Status { get; set; }
    }
}
