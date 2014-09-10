using System.Threading;
using NElasticsearch;

namespace OpsBI.Importer.ViaHttp
{
    public class PersistentPollingExecuter
    {
        private readonly ServiceControlPollingJob _job;
        private readonly ServiceControlHttpConnection _serviceControl;
        private readonly ElasticsearchRestClient _elasticsearchClient;
        private Thread thread;
        private volatile bool shuttingDown;

        public PersistentPollingExecuter(ServiceControlPollingJob job, ServiceControlHttpConnection serviceControl,
            ElasticsearchRestClient elasticsearchClient)
        {
            _job = job;
            _serviceControl = serviceControl;
            _elasticsearchClient = elasticsearchClient;
        }

        public void Start()
        {
            shuttingDown = false;
            thread = new Thread(() =>
            {
                while (!shuttingDown)
                {
                    _job.Execute(_serviceControl, _elasticsearchClient);
                }
            });
        }

        public void Shutdown()
        {
            shuttingDown = true;
            thread.Join();
        }
    }
}
