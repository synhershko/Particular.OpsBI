using System;

namespace OpsBI.Importer.ViaHttp.Models
{
    public class Endpoint
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string HostDisplayName { get; set; }
        public HearbeatInformationHolder HearbeatInformation { get; set; }

        public class HearbeatInformationHolder
        {
            public DateTime LastReportAt { get; set; }
            public string ReportedStatus { get; set; }
        }
    }
}
