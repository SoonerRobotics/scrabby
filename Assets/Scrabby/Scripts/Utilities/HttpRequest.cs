using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Scrabby.Utilities
{
    public class HttpRequest<T>
    {
        private HttpRequest(string url, string method, Dictionary<string, string> headers, string body)
        {
            _url = url;
            _method = method;
            _headers = headers;
            _body = body;
        }

        private readonly string _url;
        private readonly string _method;
        private readonly Dictionary<string, string> _headers;
        private readonly string _body;

        private Task<HttpResponse<T>> Send()
        {
            var request = new UnityWebRequest(_url, _method);
            foreach (var header in _headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            if (_body != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(_body));
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            var operation = request.SendWebRequest();
            var taskCompletionSource = new TaskCompletionSource<HttpResponse<T>>();

            operation.completed += _ =>
            {
                var response = new HttpResponse<T>(request.responseCode, default, request.error);
                if (request.responseCode is >= 200 and < 300)
                {
                    var data = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
                    response = data.TryGetValue("data", out var value)
                        ? new HttpResponse<T>(request.responseCode, value.ToObject<T>(), request.error)
                        : new HttpResponse<T>(request.responseCode, default, request.error);
                }

                taskCompletionSource.SetResult(response);
            };

            return taskCompletionSource.Task;
        }

        public static Task<HttpResponse<T>> Get(string url, Dictionary<string, string> headers)
        {
            var request = new HttpRequest<T>(url, UnityWebRequest.kHttpVerbGET, headers, null);
            return request.Send();
        }

        public static Task<HttpResponse<T>> Post(string url, Dictionary<string, string> headers, string body)
        {
            var request = new HttpRequest<T>(url, UnityWebRequest.kHttpVerbPOST, headers, body);
            return request.Send();
        }

        public static Task<HttpResponse<T>> Put(string url, Dictionary<string, string> headers, string body)
        {
            var request = new HttpRequest<T>(url, UnityWebRequest.kHttpVerbPUT, headers, body);
            return request.Send();
        }

        public static Task<HttpResponse<T>> Delete(string url, Dictionary<string, string> headers)
        {
            var request = new HttpRequest<T>(url, UnityWebRequest.kHttpVerbDELETE, headers, null);
            return request.Send();
        }
    }

    public class HttpResponse<T>
    {
        [FormerlySerializedAs("status")] public readonly long Status;

        [FormerlySerializedAs("data")] public readonly T Data;

        [FormerlySerializedAs("error")] public readonly string Error;

        public bool IsSuccess => Status >= 200 && Status < 300;

        public bool IsError => !IsSuccess;

        public HttpResponse(long status, T data, string error)
        {
            Status = status;
            Data = data;
            Error = error;
        }
    }
}