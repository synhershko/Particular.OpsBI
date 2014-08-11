using NElasticsearch.Mapping;
using OpsBI.Importer.ViaHttp.Models;

namespace OpsBI.Importer.Models
{
    [ElasticsearchType(Name = "endpoint")]
    public class EndpointHearbeatStatus
    {
        public EndpointHearbeatStatus()
        {
            
        }

        public EndpointHearbeatStatus(Endpoint endpoint)
        {
            EndpointId = endpoint.Id;
            if (endpoint.HeartbeatInformation != null)
                Status = endpoint.HeartbeatInformation.ReportedStatus;
            Name = endpoint.Name;
            Machine = endpoint.HostDisplayName;
            Address = endpoint.Address;
        }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string EndpointId { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Name { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Machine { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Address { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Status { get; set; }
    }
}
