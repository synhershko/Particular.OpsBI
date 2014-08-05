﻿using System;

namespace OpsBI.Importer.ViaHttp.Models
{
    public class MessageStatistics
    {
        public TimeSpan CriticalTime { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public TimeSpan DeliveryTime { get; set; }

        public string ElapsedCriticalTime
        {
            get { return CriticalTime.GetElapsedTime(); }
        }

        public string ElapsedProcessingTime
        {
            get { return ProcessingTime.GetElapsedTime(); }
        }

        public string ElapsedDeliveryTime
        {
            get { return DeliveryTime.GetElapsedTime(); }
        }
    }
}
