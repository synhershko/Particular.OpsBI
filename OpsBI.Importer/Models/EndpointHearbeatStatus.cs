using OpsBI.Importer.ViaHttp.Models;

namespace OpsBI.Importer.Models
{
    public class EndpointHearbeatStatus
    {
        public EndpointHearbeatStatus()
        {
            
        }

        public EndpointHearbeatStatus(Endpoint endpoint)
        {
            EndpointId = endpoint.Id;
            Status = endpoint.HeartbeatInformation.ReportedStatus;
            Name = endpoint.Name;
            Machine = endpoint.HostDisplayName;
            Address = endpoint.Address;
        }

        public string EndpointId { get; set; }
        public string Name { get; set; }
        public string Machine { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
    }
}
