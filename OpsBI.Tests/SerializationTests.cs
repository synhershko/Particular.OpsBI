using System.Collections.Generic;
using System.IO;
using OpsBI.Importer.ViaHttp.Models;
using RestSharp;
using RestSharp.Deserializers;
using Xunit;

namespace OpsBI.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void Deserializes_messages_correctly()
        {
            var deserializer = new JsonDeserializer();
            var response = new RestResponse();
            response.Content = File.ReadAllText(@"Z:\code\Particular\Particular.OpsBI\OpsBI.Tests\Data\FailedMessages.json");

            var r = deserializer.Deserialize<List<StoredMessage>>(response);
        }
    }
}
