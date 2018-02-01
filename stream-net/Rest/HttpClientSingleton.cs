using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StreamNetDisposable.Rest
{
    public class HttpClientSingleton 
    {

        private static readonly HttpClient _instance = new HttpClient();

        static HttpClientSingleton() {
         
            _instance.Timeout = TimeSpan.FromMilliseconds(15000);
            _instance.DefaultRequestHeaders.Accept.Clear();
            _instance.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _instance.DefaultRequestHeaders.ConnectionClose = false; // ensure connections being reused
        }

        private HttpClientSingleton()
        { }

        public static HttpClient Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
