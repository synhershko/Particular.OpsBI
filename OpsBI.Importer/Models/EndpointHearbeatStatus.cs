using NElasticsearch.Mapping;
using OpsBI.Importer.ViaHttp.Models;
using Quartz.Util;

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

            if (endpoint.Monitored == false)
            {
                Status = "Not Monitored";
            }
            else // endpoint.Monitored == true
            {
                if (endpoint.IsSendingHeartbeats == false)
                    Status = "Plugin Not Installed";
                else
                {
                    if (endpoint.HeartbeatInformation != null && !endpoint.HeartbeatInformation.ReportedStatus.IsNullOrWhiteSpace())
                        Status = endpoint.HeartbeatInformation.ReportedStatus;
                    else
                        Status = "dead";
                }
            }

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
