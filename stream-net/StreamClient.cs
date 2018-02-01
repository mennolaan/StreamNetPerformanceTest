﻿using Newtonsoft.Json;
using StreamNetDisposable.Rest;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StreamNetDisposable
{
    public class StreamClient : IDisposable
    {
        internal const string BaseUrlFormat = "https://{0}-api.stream-io-api.com";
        internal const string BaseUrlPath = "/api/v1.0/";
        internal const string ActivitiesUrlPath = "activities/";
        internal const int ActivityCopyLimitDefault = 300;
        internal const int ActivityCopyLimitMax = 1000;

        private RestClient _restClient;
        readonly StreamClientOptions _options;
        readonly string _apiSecret;
        readonly string _apiKey;

        public StreamClient(string apiKey, string apiSecret, StreamClientOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException("apiKey", "Must have an apiKey");
            if (string.IsNullOrWhiteSpace(apiSecret))
                throw new ArgumentNullException("apiSecret", "Must have an apiSecret");

            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _options = options ?? StreamClientOptions.Default;
            _restClient = new RestClient(GetBaseUrl(), TimeSpan.FromMilliseconds(_options.Timeout));
#if NET45
            ServicePointManager.FindServicePoint(GetBaseUrl()).ConnectionLeaseTimeout = 60000;
            ServicePointManager.DnsRefreshTimeout = 60000;
#endif
        }

        /// <summary>
        /// Get a feed
        /// </summary>
        /// <param name="feedSlug"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public StreamFeed Feed(string feedSlug, string userId)
        {
            // handle required arguments
            if (string.IsNullOrWhiteSpace(feedSlug))
                throw new ArgumentNullException("feedSlug", "Must have a feedSlug");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("userId", "Must have an userId");

            string token = Sign(feedSlug + userId);
            return new StreamFeed(this, feedSlug, userId, token);
        }

        /// <summary>
        /// Access batch operations
        /// </summary>
        public BatchOperations Batch
        {
            get
            {
                return new BatchOperations(this);
            }
        }

        private Uri GetBaseUrl()
        {
            string region = "";
            switch (_options.Location)
            {
                case StreamApiLocation.USEast:
                    region = "us-east";
                    break;
                case StreamApiLocation.USWest:
                    region = "us-west";
                    break;
                case StreamApiLocation.EUCentral:
                    region = "eu-central";
                    break;
                default:
                    break;
            }
            return new Uri(string.Format(BaseUrlFormat, region));
        }

        private RestRequest BuildRestRequest(string fullPath, HttpMethod method)
        {
            var request = new RestRequest(fullPath, method);
            request.AddHeader("Authorization", JWToken("*"));
            request.AddHeader("stream-auth-type", "jwt");
            request.AddQueryParameter("api_key", _apiKey);
            return request;
        }

        internal RestRequest BuildFeedRequest(StreamFeed feed, string path, HttpMethod method)
        {
            return BuildRestRequest(BaseUrlPath + feed.UrlPath + path, method);
        }

        internal RestRequest BuildActivitiesRequest(StreamFeed feed)
        {
            return BuildRestRequest(BaseUrlPath + ActivitiesUrlPath, HttpMethod.POST);
        }

        internal RestRequest BuildAppRequest(string path, HttpMethod method)
        {
            var request = new RestRequest(BaseUrlPath + path, method);
            request.AddHeader("X-Api-Key", _apiKey);
            return request;
        }

        internal void SignRequest(RestRequest request)
        {
            // make signature
            var queryString = "";
            request.QueryParameters.ForEach((p) =>
            {   
                queryString += (queryString.Length == 0) ? "?" : "&";
                queryString += string.Format("{0}={1}", p.Key, Uri.EscapeDataString(p.Value.ToString()));
            });
            var toSign = string.Format("(request-target): {0} {1}", request.Method.ToString().ToLower(), request.Resource + queryString);

            var signature = string.Format("keyId=\"{0}\",algorithm=\"hmac-sha256\",headers=\"(request-target)\",signature=\"{1}\"", this._apiKey, Sign256(toSign));
            request.AddHeader("Authorization", "Signature " + signature);
        }

        internal Task<RestResponse> MakeRequest(RestRequest request)
        {
            return _restClient.Execute(request);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Trim('=');
        }

        internal string Sign(string feedId)
        {
            Encoding encoding = new ASCIIEncoding();
#if NETCORE
            var hashedSecret = SHA1.Create().ComputeHash(encoding.GetBytes(_apiSecret));
#else
            var hashedSecret = (new SHA1Managed()).ComputeHash(encoding.GetBytes(_apiSecret));
#endif

            var hmac = new HMACSHA1(hashedSecret);
            return Base64UrlEncode(hmac.ComputeHash(encoding.GetBytes(feedId)));
        }

        internal string Sign256(string feedId)
        {
            Encoding encoding = new ASCIIEncoding();
            var hmac = new HMACSHA256(encoding.GetBytes(_apiSecret));
            return Convert.ToBase64String(hmac.ComputeHash(encoding.GetBytes(feedId)));
        }

        internal string JWToken(string feedId)
        {
            var segments = new List<string>();
            var header = new {
                typ = "JWT",
                alg = "HS256" 
            };
            var noTimestamp = !this._options.ExpireTokens;
            var payload = new
            {
                resource = "*",
                action = "*",
                feed_id = feedId
            };

            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header));
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));

            segments.Add(Base64UrlEncode(headerBytes));
            segments.Add(Base64UrlEncode(payloadBytes));
            
            var stringToSign = string.Join(".", segments.ToArray());
            var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            using (var sha = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret)))
            {
                byte[] signature = sha.ComputeHash(bytesToSign);
                segments.Add(Base64UrlEncode(signature));
            }            
            return string.Join(".", segments.ToArray());
        }

        internal string SignTo(string to)
        {
            string[] bits = to.Split(':');
            var otherFeed = this.Feed(bits[0], bits[1]);
            return to + " " + otherFeed.Token;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_restClient != null)
                {
                    _restClient.Dispose();
                    _restClient = null;
                }
            }
        }
    }
}
