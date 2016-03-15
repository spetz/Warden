using System;

namespace Sentry.Watchers.Api
{
    public class HttpRequest
    {
        public MethodType Type { get; }
        public string Endpoint { get;}
        public object Data { get; }

        protected HttpRequest(MethodType type, string endpoint, object data = null)
        {
            if(string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint can not be empty.", nameof(endpoint));

            Type = type;
            Endpoint = endpoint;
            Data = data;
        }

        public static HttpRequest Get(string endpoint) => new HttpRequest(MethodType.Get, endpoint);
        public static HttpRequest Put(string endpoint, object data = null) => new HttpRequest(MethodType.Put, endpoint, data);
        public static HttpRequest Post(string endpoint, object data = null) => new HttpRequest(MethodType.Post, endpoint, data);
        public static HttpRequest Delete(string endpoint) => new HttpRequest(MethodType.Delete, endpoint);

        public enum MethodType
        {
            Get = 1,
            Put = 2,
            Post = 3,
            Delete = 4
        }
    }
}