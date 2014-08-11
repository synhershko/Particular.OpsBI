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
            this.Status = message.Status;
        }

        public Message()
        {
            
        }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed, Name = "message_id")]
        public string MessageId { get; set; }
        
        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Type { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed, Name = "sending_endpoint_address")]
        public string SendingEndpointAddress { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed, Name = "receiving_endpoint_address")]
        public string ReceivingEndpointAddress { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.No, Name = "platform_url")]
        public string PlatformUrl { get; set; }

        [ElasticsearchProperty(Name = "is_failed")]
        public bool IsFailed { get; set; }

        [ElasticsearchProperty(Name = "time_sent")]
        public DateTime TimeSent { get; set; }

        [ElasticsearchProperty(Name = "processed_at")]
        public DateTime ProcessedAt { get; set; }

        [ElasticsearchProperty(Name = "delivery_time")]
        public int DeliveryTime { get; set; }

        [ElasticsearchProperty(Name = "processing_time")]
        public int ProcessingTime { get; set; }

        public MessageStatus Status { get; set; }
    }
}
