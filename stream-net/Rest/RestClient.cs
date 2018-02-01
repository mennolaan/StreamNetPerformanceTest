using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StreamNetDisposable.Rest
{
    internal class RestClient : IDisposable
    {
        readonly Uri _baseUrl;
        private TimeSpan _timeout;

        public RestClient(Uri baseUrl, TimeSpan timeout)
        {
            _baseUrl = baseUrl;
            _timeout = timeout;
          
            HttpClientSingleton.Instance.Timeout = _timeout;
             #if NET45
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
              
            #endif
        }   

        private Uri BuildUri(RestRequest request)
        {
            var queryString = "";
            request.QueryParameters.ForEach((p) =>
            {
                queryString += (queryString.Length == 0) ? "?" : "&";
                queryString += string.Format("{0}={1}", p.Key, Uri.EscapeDataString(p.Value.ToString()));
            });
            return new Uri(_baseUrl, request.Resource + queryString);
        }

        public async Task<RestResponse> Execute(RestRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request", "Request is required");
           
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = BuildUri(request)
            };

            // setup method and content if needed
            switch (request.Method)
            {
                case HttpMethod.DELETE:
                    httpRequest.Method = System.Net.Http.HttpMethod.Delete;
                    break;
                case HttpMethod.POST:                    
                    httpRequest.Method = System.Net.Http.HttpMethod.Post;
                    httpRequest.Content = new StringContent(request.JsonBody, Encoding.UTF8, "application/json");
                    break;                    
                default:
                    httpRequest.Method = System.Net.Http.HttpMethod.Get;
                    break;
            }

            // add request headers
            httpRequest.Headers.Clear(); // should be clear already as its newed in this task
            request.Headers.ForEach(h =>
            {
                httpRequest.Headers.Add(h.Key, h.Value);
            });

            using(var response = await HttpClientSingleton.Instance.SendAsync(httpRequest))
            {
                return await RestResponse.FromResponseMessage(response);
            }
        }              

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
