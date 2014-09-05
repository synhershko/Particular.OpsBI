using System;
using Nancy.Hosting.Self;

namespace OpsBI.Tests.Infrastructure
{
    public abstract class EndToEndTestBase : IDisposable
    {
        protected readonly Uri uri;
        private readonly NancyHost host;

        protected EndToEndTestBase(int port = 8989)
        {
            uri = new Uri("http://localhost:" + port);
            host = new NancyHost(new HostConfiguration {UrlReservations = new UrlReservations {CreateAutomatically = true}}, uri);
            host.Start();
        }

        public void Dispose()
        {
            host.Dispose();
        }
    }
}
