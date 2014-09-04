using System;

namespace OpsBI.Importer.ViaHttp.Models
{
    public class CustomCheck
    {
        public string Id { get; set; }
        public string CustomCheckId { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public DateTime ReportedAt { get; set; }
        public string FailureReason { get; set; }
        public EndpointDetails OriginatingEndpoint { get; set; }

        public class EndpointDetails
        {
            public string Name { get; set; }
            public string HostId { get; set; }
            public string Host { get; set; }

            public string Address
            {
                get { return string.Format("{0}{1}", Name, AtMachine()); }
            }

            string AtMachine()
            {
                return string.IsNullOrEmpty(Host) ? string.Empty : string.Format("@{0}", Host);
            }
        }
    }
}
