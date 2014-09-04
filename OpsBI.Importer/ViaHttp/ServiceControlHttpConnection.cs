using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Anotar.Serilog;
using OpsBI.Importer.Models;
using OpsBI.Importer.ViaHttp.Models;
using RestSharp;
using RestSharp.Contrib;
using RestSharp.Deserializers;
using Serilog;

namespace OpsBI.Importer.ViaHttp
{
    public class ServiceControlHttpConnection : IServiceControl
    {
        private readonly string _url;
        private static ILogger AnotarLogger = Log.ForContext<IServiceControl>();

        private const string ConversationEndpoint = "conversations/{0}";
        private const string EndpointsEndpoint = "endpoints";
        private const string EndpointMessagesEndpoint = "endpoints/{0}/messages/";
        private const string RetryEndpoint = "errors/{0}/retry";
        private const string MessagesEndpoint = "messages.json";
        private const string MessageBodyEndpoint = "messages/{0}/body";
        private const string SagaEndpoint = "sagas/{0}";
        private const string CustomChecksEndpoint = "customchecks";

        public ServiceControlHttpConnection(string url)
        {
            _url = url;
        }

        public bool IsAlive()
        {
            return GetVersion() != null;
        }

        public string GetVersion()
        {
            var request = new RestRequest();

            LogRequest(request);

            var response = CreateClient().Execute(request);
            return ProcessResponse(restResponse => restResponse.Headers.First(x => x.Name == ServiceControlHeaders.ParticularVersion).Value.ToString(), response);
        }

        public void RetryMessage(string messageId)
        {
            var url = string.Format(RetryEndpoint, messageId);
            var request = new RestRequest(url, Method.POST);
            Execute(request, HasSucceeded);
        }

        public Uri CreateServiceInsightUri(StoredMessage message)
        {
            var connectionUri = new Uri(_url);
            return new Uri(string.Format("si://{0}:{1}/api{2}", connectionUri.Host, connectionUri.Port, message.GetURIQuery()));
        }

        public SagaData GetSagaById(Guid sagaId)
        {
            return GetModel<SagaData>(CreateSagaRequest(sagaId)) ?? new SagaData();
        }

        public PagedResult<StoredMessage> Search(string searchQuery, int pageIndex = 1, string orderBy = null, bool ascending = false)
        {
            var request = CreateMessagesRequest();

            AppendSystemMessages(request);
            AppendSearchQuery(request, searchQuery);
            AppendPaging(request, pageIndex);
            AppendOrdering(request, orderBy, ascending);

            var result = GetPagedResult<StoredMessage>(request);
            result.CurrentPage = pageIndex;

            return result;
        }

        public IEnumerable<CustomCheck> GetCustomChecks()
        {
            IRestRequest request = new RestRequest(CustomChecksEndpoint) { RequestFormat = DataFormat.Json };
            LogRequest(request);

            var response = CreateClient().Execute<List<CustomCheck>>(request);

            if (HasSucceeded(response))
            {
                LogResponse(response);
                return response.Data;
            }
            
            LogError(response);
            return Enumerable.Empty<CustomCheck>();
        }

        public PagedResult<StoredMessage> GetAuditMessages(int pageIndex = 1, string orderBy = null, bool ascending = false, string searchQuery = null, Endpoint endpoint = null)
        {
            IRestRequest request = endpoint == null ? CreateMessagesRequest() : CreateMessagesRequest(endpoint.Name);

            AppendSystemMessages(request);
            AppendSearchQuery(request, searchQuery);
            AppendPaging(request, pageIndex);
            AppendOrdering(request, orderBy, ascending);

            var result = GetPagedResult<StoredMessage>(request);
            result.CurrentPage = pageIndex;

            return result;
        }

        PagedResult<T> GetPagedResult<T>(IRestRequest request) where T : class, new()
        {
            LogRequest(request);

            var response = CreateClient().Execute<List<T>>(request);

            if (HasSucceeded(response))
            {
                LogResponse(response);
                return new PagedResult<T>
                {
                    Result = response.Data,
                    TotalCount = int.Parse(response.Headers.First(x => x.Name == ServiceControlHeaders.TotalCount).Value.ToString())
                };
            }
            else
            {
                LogError(response);
                return new PagedResult<T>();
            }
        }

        public IEnumerable<StoredMessage> GetConversationById(string conversationId)
        {
            var request = new RestRequest(string.Format(ConversationEndpoint, conversationId));
            var messages = GetModel<List<StoredMessage>>(request) ?? new List<StoredMessage>();

            return messages;
        }

        public IEnumerable<Endpoint> GetEndpoints()
        {
            var request = new RestRequest(EndpointsEndpoint);
            var messages = GetModel<List<Endpoint>>(request);

            return messages ?? new List<Endpoint>();
        }

        public IEnumerable<KeyValuePair<string, string>> GetMessageData(Guid messageId)
        {
//            var request = new RestRequest(String.Format(MessageBodyEndpoint, messageId));
//
//            return Execute(request, response =>
//                response.Content.StartsWith("<?xml") ?
//                    GetXmlData(response.Content) :
//                    JsonPropertiesHelper.ProcessValues(response.Content, CleanupBodyString));
            throw new NotImplementedException();
        }

        public void LoadBody(StoredMessage message)
        {
            var client = message.BodyUrl.StartsWith("http") ? CreateClient(message.BodyUrl) : CreateClient();

            var request = new RestRequest(message.BodyUrl, Method.GET);

            message.Body = Execute(client, request, response => response.Content);
        }

        void AppendSystemMessages(IRestRequest request)
        {
            request.AddParameter("include_system_messages", false); // TODO use config to decide on that?
        }

        void AppendOrdering(IRestRequest request, string orderBy, bool ascending)
        {
            if (orderBy == null) return;
            request.AddParameter("sort", orderBy, ParameterType.GetOrPost);
            request.AddParameter("direction", ascending ? "asc" : "desc", ParameterType.GetOrPost);
        }

        void AppendPaging(IRestRequest request, int pageIndex)
        {
            request.AddParameter("page", pageIndex, ParameterType.GetOrPost);
        }

        void AppendSearchQuery(IRestRequest request, string searchQuery)
        {
            if (searchQuery == null) return;
            request.Resource += string.Format("search/{0}", HttpUtility.UrlEncode(searchQuery));
        }

        IRestClient CreateClient()
        {
            return CreateClient(_url);
        }

        IRestClient CreateClient(string url)
        {
            var client = new RestClient(url);
            var deserializer = new JsonMessageDeserializer();
            var xdeserializer = new XmlDeserializer();
            client.ClearHandlers();
            client.AddHandler("application/json", deserializer);
            client.AddHandler("text/json", deserializer);
            client.AddHandler("text/x-json", deserializer);
            client.AddHandler("text/javascript", deserializer);

            client.AddHandler("application/xml", xdeserializer);
            client.AddHandler("text/xml", xdeserializer);
            client.AddHandler("*", xdeserializer);

            client.AddDefaultHeader("Accept-Encoding", "gzip,deflate");

            return client;
        }

        static RestRequest CreateSagaRequest(Guid sagaId)
        {
            return new RestRequest(string.Format(SagaEndpoint, sagaId));
        }

        static RestRequest CreateMessagesRequest(string endpointName = null)
        {
            return endpointName != null ?
                new RestRequest(string.Format(EndpointMessagesEndpoint, endpointName)){RequestFormat = DataFormat.Json}
                :
                new RestRequest(MessagesEndpoint) { RequestFormat = DataFormat.Json };
        }

        T GetModel<T>(IRestRequest request)
            where T : class, new()
        {
            return Execute<T>(request, response => response.Data);
        }

        T Execute<T>(IRestRequest request, Func<IRestResponse, T> selector)
        {
            return Execute(CreateClient(), request, selector);
        }

        T Execute<T>(IRestClient client, IRestRequest request, Func<IRestResponse, T> selector)
        {
            LogRequest(request);

            var response = client.Execute(request);
            return ProcessResponse(selector, response);
        }

        T Execute<T>(IRestRequest request, Func<IRestResponse<T>, T> selector)
            where T : class, new()
        {
            LogRequest(request);

            var response = CreateClient().Execute<T>(request);
            return ProcessResponse(selector, response);
        }      

        T ProcessResponse<T>(Func<IRestResponse, T> selector, IRestResponse response)
        {
            if (HasSucceeded(response))
            {
                LogResponse(response);
                return selector(response);
            }
            else
            {
                LogError(response);
                return default(T);
            }
        }

        T ProcessResponse<T>(Func<IRestResponse<T>, T> selector, IRestResponse<T> response)
        {
            if (HasSucceeded(response))
            {
                LogResponse(response);
                return selector(response);
            }
            else
            {
                LogError(response);
                return default(T);
            }
        }

        IEnumerable<KeyValuePair<string, string>> GetXmlData(string bodyString)
        {
            try
            {
                var xml = XDocument.Parse(bodyString);
                if (xml.Root != null)
                {
                    var root = xml.Root.Nodes().FirstOrDefault() as XElement;
                    if (root != null)
                    {
                        return root.Nodes()
                                   .OfType<XElement>()
                                   .Select(n => new KeyValuePair<string, string>(n.Name.LocalName, n.Value));
                    }
                }
            }
            catch (XmlException) { }
            return new List<KeyValuePair<string, string>>();
        }

        static string CleanupBodyString(string bodyString)
        {
            return bodyString.Replace("\u005c", string.Empty).Replace("\uFEFF", string.Empty).TrimStart("[\"".ToCharArray()).TrimEnd("]\"".ToCharArray());
        }

        void LogRequest(IRestRequest request)
        {
            var resource = request.Resource != null ? request.Resource.TrimStart('/') : string.Empty;
            var url = _url != null ? _url.TrimEnd('/') : string.Empty;

            LogTo.Information("HTTP {Method} {url:l}/{resource:l}", request.Method, url, resource);

            foreach (var parameter in request.Parameters)
            {
                LogTo.Debug("Request Parameter: {Name} : {Value}",
                                                       parameter.Name,
                                                       parameter.Value);
            }
        }

        void LogResponse(IRestResponse response)
        {
            var code = response.StatusCode;
            var uri = response.ResponseUri;

            LogTo.Debug("HTTP Status {code} ({uri})", code, uri);

            foreach (var header in response.Headers)
            {
                LogTo.Debug("Response Header: {Name} : {Value}",
                                                     header.Name,
                                                     header.Value);
            }
        }

        void LogError(IRestResponse response)
        {
            var exception = response != null ? response.ErrorException : null;
            var errorMessage = response != null ? string.Format("Error executing the request: {0}, Status code is {1}", response.ErrorMessage, response.StatusCode) : "No response was received.";

            //eventAggregator.Publish(new AsyncOperationFailed(errorMessage));
            LogTo.Error(exception, errorMessage);
        }

        static bool HasSucceeded(IRestResponse response)
        {
            return SuccessCodes.Any(x => response != null && x == response.StatusCode && response.ErrorException == null);
        }

        static readonly IEnumerable<HttpStatusCode> SuccessCodes = new[] { HttpStatusCode.OK, HttpStatusCode.Accepted };
    }
}
