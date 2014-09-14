using System;

namespace OpsBI.Importer.ViaHttp.Models
{
    public class Endpoint
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Host
        {
            get { return HostDisplayName; }
            set { HostDisplayName = value; }
        }

        public string HostDisplayName { get; set; }
        public HeartbeatInformationHolder HeartbeatInformation { get; set; }

        public string Address
        {
            get { return string.Format("{0}{1}", Name, AtMachine()); }
        }

        string AtMachine()
        {
            return string.IsNullOrEmpty(HostDisplayName) ? string.Empty : string.Format("@{0}", HostDisplayName);
        }

        public class HeartbeatInformationHolder
        {
            public DateTime LastReportAt { get; set; }
            public string ReportedStatus { get; set; }
        }
    }
}
