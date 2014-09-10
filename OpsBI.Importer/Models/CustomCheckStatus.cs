using NElasticsearch.Mapping;
using OpsBI.Importer.ViaHttp.Models;

namespace OpsBI.Importer.Models
{
    [ElasticsearchType(Name = "customcheck")]
    public class CustomCheckStatus
    {
        public CustomCheckStatus(CustomCheck customCheck)
        {
            CustomCheckId = customCheck.Id;
            Name = customCheck.CustomCheckId;
            Category = customCheck.Category;            
            Status = customCheck.Status;
            FullName = string.Format("{0} {1}", Name, OriginatingEndpoint);
            OriginatingEndpoint = customCheck.OriginatingEndpoint.Address;
            HostName = customCheck.OriginatingEndpoint.Host;
        }

        public CustomCheckStatus()
        {

        }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string CustomCheckId { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Name { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string FullName { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Category { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string OriginatingEndpoint { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string HostName { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Status { get; set; }
    }
}
