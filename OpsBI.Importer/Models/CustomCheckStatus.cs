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
            OriginatingEndpoint = customCheck.OriginatingEndpoint.Address;
            Status = customCheck.Status;
        }

        public CustomCheckStatus()
        {

        }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string CustomCheckId { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Name { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Category { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string OriginatingEndpoint { get; set; }

        [ElasticsearchProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Status { get; set; }
    }
}
