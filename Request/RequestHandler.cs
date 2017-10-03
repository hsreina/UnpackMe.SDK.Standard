using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using UnpackMe.SDK.Core.Exceptions;
using UnpackMe.SDK.Core.Models;
using UnpackMe.SDK.Core.Extensions;
using System.Threading.Tasks;
using System.Text;

namespace UnpackMe.SDK.Core.Request
{
    class RequestHandler : IDisposable
    {
        private HttpClient _client;

        private string _serviceUrl;

        public RequestHandler(string serviceUrl, string basicLogin = null, string basicPassword = null)
        {
            _client = new HttpClient();
            _serviceUrl = serviceUrl;

            if (String.IsNullOrEmpty(basicLogin) || String.IsNullOrEmpty(basicPassword))
            {
                return;
            }

            var byteArray = Encoding.ASCII.GetBytes(String.Format("{0}:{1}", basicLogin, basicPassword));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public T Post<T>(string path, KeyValuePair<string, string>[] parameters = null)
        {
            var formUrlEncodedContent = new FormUrlEncodedContent(parameters);
            var data = _client.PostAsync(String.Format("{0}{1}", _serviceUrl, path), formUrlEncodedContent).Result;
            var jsonResult = data.Content.ReadAsStringAsync().Result;
            GuardAgainstInvalidStatus(data.StatusCode, jsonResult);
            return JsonConvert.DeserializeObject<T>(jsonResult);
        }

        public T PostStream<T>(string path, Stream stream)
        {
            var multipart = new MultipartFormDataContent();
            var streamContent = new StreamContent(stream);

            multipart.Add(streamContent);
            multipart.Add(streamContent, "file");

            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "test"
            };

            var data = _client.PostAsync(String.Format("{0}{1}", _serviceUrl, path), multipart).Result;
            var jsonResult = data.Content.ReadAsStringAsync().Result;
            GuardAgainstInvalidStatus(data.StatusCode, jsonResult);
            return JsonConvert.DeserializeObject<T>(jsonResult);
        }

        public T Get<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(Get(path));
        }

        public string Get(string path)
        {
            var data = _client.GetAsync(String.Format("{0}{1}", _serviceUrl, path)).Result;
            var jsonResult = data.Content.ReadAsStringAsync().Result;
            GuardAgainstInvalidStatus(data.StatusCode, jsonResult);
            return jsonResult;
        }

        public void GetIntoFile(string path, string filename)
        {
            GetIntoFileAsync(path, filename).Wait();
        }

        public Task GetIntoFileAsync(string path, string filename)
        {
            var data = _client.GetAsync(String.Format("{0}{1}", _serviceUrl, path)).Result;
            if (data.StatusCode != HttpStatusCode.OK)
            {
                GuardAgainstInvalidStatus(data.StatusCode, data.Content.ReadAsStringAsync().Result);
            }
            return data.Content.ReadAsFileAsync(filename);
        }

        public void SetToken(string token)
        {
            _client.DefaultRequestHeaders.Remove("Token");
            _client.DefaultRequestHeaders.Add("Token", token);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private void GuardAgainstInvalidStatus(HttpStatusCode StatusCode, string jsonResult)
        {
            if (StatusCode != HttpStatusCode.OK)
            {
                var error = JsonConvert.DeserializeObject<ErrorModel>(jsonResult);
                throw new UnpackMeException(error.ErrorMessage);
            }
        }
    }
}
