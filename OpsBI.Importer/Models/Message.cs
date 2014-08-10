using System;
using OpsBI.Importer.ViaHttp.Models;

namespace OpsBI.Importer.Models
{
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

        public string MessageId { get; set; }
        public string Type { get; set; }
        public string SendingEndpointAddress { get; set; }
        public string ReceivingEndpointAddress { get; set; }
        public string ServiceInsightUrl { get; set; }
        public bool IsFailed { get; set; }
        public DateTime TimeSent { get; set; }
        public DateTime ProcessedAt { get; set; }
        public int DeliveryTime { get; set; }
        public int ProcessingTime { get; set; }
        public MessageStatus Status { get; set; }
    }
}
