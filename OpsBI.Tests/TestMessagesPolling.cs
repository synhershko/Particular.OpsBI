using NElasticsearch;
using OpsBI.Importer.ViaHttp;
using OpsBI.Tests.Infrastructure;
using Xunit;

namespace OpsBI.Tests
{
    public class TestMessagesPolling : EndToEndTestBase
    {
        [Fact]
        public void Polls_correctly()
        {
            var serviceControl = new ServiceControlHttpConnection(uri.ToString());
            var elasticsearch = new ElasticsearchRestClient(uri);

            var poller = new ReportToElasticsearch.MessagePolling();

            poller.Execute(serviceControl, elasticsearch);
        }
    }
}
