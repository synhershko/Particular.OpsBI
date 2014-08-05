using System;
using System.Collections.Generic;
using System.Linq;

namespace OpsBI.Importer.ViaHttp.Models
{
    public class SagaUpdate
    {
        public DateTime FinishTime { get; set; }

        public DateTime StartTime { get; set; }

        public SagaStateChangeStatus Status { get; set; }

        public SagaMessage InitiatingMessage { get; set; }

        public List<SagaTimeoutMessage> OutgoingMessages { get; set; }

        public bool IsFirstNode
        {
            get { return Status == SagaStateChangeStatus.New; }
        }

        public bool IsSagaTimeoutMessage
        {
            get { return InitiatingMessage.IsSagaTimeoutMessage; }
        }

        public List<SagaMessage> NonTimeoutMessages
        {
            get { return OutgoingMessages.Where(m => !m.IsTimeout).Cast<SagaMessage>().ToList(); }
        }

        public List<SagaTimeoutMessage> TimeoutMessages
        {
            get { return OutgoingMessages.Where(m => m.IsTimeout).ToList(); }
        }

        public bool HasNonTimeoutMessages { get { return NonTimeoutMessages.Any(); } }

        public bool HasTimeoutMessages { get { return TimeoutMessages.Any(); } }

        public string StateAfterChange { get; set; }

        public string Label
        {
            get
            {
                switch (Status)
                {
                    case SagaStateChangeStatus.New:
                        return "Saga Initiated";

                    case SagaStateChangeStatus.Completed:
                    case SagaStateChangeStatus.Updated:
                        return "Saga Updated";
                }

                return string.Empty;
            }
        }
    }

    public enum SagaStateChangeStatus
    {
        Nothing,
        New,
        Updated,
        Completed
    }
}
