﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OpsBI.Importer.ViaHttp.Models
{
    public class SagaMessage
    {
        public Guid MessageId { get; set; }

        public bool IsPublished { get; set; }

        public virtual bool IsTimeout
        {
            get
            {
                return IsSagaTimeoutMessage;
            }
        }

        public bool IsSagaTimeoutMessage { get; set; } //for SC, not to be confused with timeout outgoing messages

        public string MessageType
        {
            get;
            set;
        }

        public string MessageFriendlyTypeName
        {
            get { return ProcessType(); }
        }

        string ProcessType()
        {
            if (string.IsNullOrEmpty(MessageType))
                return string.Empty;

            var clazz = MessageType.Split(',').First();
            var objectName = clazz.Split('.').Last();

            if (objectName.Contains("+"))
                objectName = objectName.Split('+').Last();

            return objectName;
        }

        public DateTime TimeSent { get; set; }

        public string ReceivingEndpoint { get; set; }

        public string OriginatingEndpoint { get; set; }

        MessageStatus status;

        public MessageStatus Status
        {
            get
            {
                return status == 0 ? MessageStatus.Successful : status;
            }
            set
            {
                status = value;
            }
        }

        List<KeyValuePair<MessageStatus, string>> statuses = new List<KeyValuePair<MessageStatus, string>> {
            new KeyValuePair<MessageStatus, string>(MessageStatus.Failed, "Fail" ),
            new KeyValuePair<MessageStatus, string>(MessageStatus.RepeatedFailure, "RepeatedFail" ),
            new KeyValuePair<MessageStatus, string>(MessageStatus.RetryIssued, "Retry" ),
            new KeyValuePair<MessageStatus, string>(MessageStatus.Successful, "Success" ),
        };

        public bool IsSelected { get; set; }

        public bool HasFailed
        {
            get
            {
                return (Status == MessageStatus.Failed) || (Status == MessageStatus.RepeatedFailure);
            }
        }

        public string StatusText
        {
            get
            {
                return statuses.FirstOrDefault(k => k.Key == Status).Value;
            }
            set
            {
                Status = statuses.FirstOrDefault(k => k.Value == value).Key;
            }
        }

        public bool HasRetried
        {
            get
            {
                return Status == MessageStatus.RetryIssued;
            }
        }

        bool showData;

        public bool ShowData
        {
            get
            {
                return showData && Data != null && Data.Any();
            }
            set
            {
                showData = value;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Data { get; private set; }

        internal void RefreshData(IServiceControl serviceControl)
        {
            if (Data != null) return;

            Data = serviceControl.GetMessageData(MessageId);
        }
    }

    public class SagaTimeoutMessage : SagaMessage
    {
        public SagaTimeoutMessage()
        {
            DeliverAt = DateTime.MinValue;
            Timeout = TimeSpan.MinValue;
        }

        public TimeSpan Timeout { get; set; }

        public override bool IsTimeout
        {
            get
            {
                return (DeliverAt != DateTime.MinValue || Timeout != TimeSpan.MinValue);
            }
        }

        public DateTime DeliverAt { get; set; }

        public string DeliveryDelay
        {
            get
            {
                return Timeout.ToString(@"hh\:mm\:ss");
            }
            set
            {
                Timeout = TimeSpan.Parse(value);
            }
        }

        public string TimeoutFriendly
        {
            get
            {
                if (Timeout != TimeSpan.MinValue)
                    return string.Format("{0}{1}{2}{3}", GetFriendly(Timeout.Days, "d"), GetFriendly(Timeout.Hours, "h"), GetFriendly(Timeout.Minutes, "m"), GetFriendly(Timeout.Seconds, "s"));

                return DeliverAt.ToString();
            }
        }

        string GetFriendly(int time, string text)
        {
            if (time > 0)
            {
                return string.Format("{0}{1}", time, text);
            }
            return string.Empty;
        }
    }
}
