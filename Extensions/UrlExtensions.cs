using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using KuraSharp.Data;
using Newtonsoft.Json;
using Serilog;

namespace KuraSharp.Extensions;

public static class UrlExtensions {
    static readonly ILogger Logger = Log.Logger.ForType(typeof(UrlExtensions));
    
    [Obsolete($"Use JsonHttpRequest instead")]
    public static async Task<HttpResponseMessage> GetRequest(Uri uri) {
        using (HttpClient httpClient = new())
            return await httpClient.GetAsync(uri);
    }

    public static async Task<BaseData> JsonHttpRequest(string uri, HttpMethod httpMethod, Dictionary<string, string>? jsonData, Dictionary<string, string>? headers) {
        HttpRequestMessage httpRequestMessage = new() {
            Method = httpMethod,
            RequestUri = new Uri(uri),
            Headers = {
                { HttpRequestHeader.Accept.ToString(), "application/json" }
            }
        };

        if (jsonData != null) {
            string serializedData = JsonConvert.SerializeObject(jsonData);
            httpRequestMessage.Content = new StringContent(serializedData, Encoding.UTF8, "application/json");
        }

        headers?.ToList().ForEach(h => httpRequestMessage.Headers.Add(h.Key, h.Value));

        using (HttpClient httpClient = new()) {
            HttpResponseMessage responseMessage = await httpClient.SendAsync(httpRequestMessage);

            BaseData responseData = new() {
                IsOk = responseMessage.IsSuccessStatusCode,
                Data = responseMessage.Content
            };

            return responseData;
        }
    }

    [Obsolete("не работает йопта")]
    public static BaseData MultipartHttpRequest(string uri, HttpMethod httpMethod, Dictionary<string, string>? headers, Dictionary<string, string>? stringData, List<byte[]>? files) {
        MultipartFormDataContent multipartFormDataContent = new();
        
        stringData?.ToList().ForEach(s => multipartFormDataContent.Add(new StringContent(s.Value), s.Key));
        files?.ForEach(f => multipartFormDataContent.Add(new StreamContent(new MemoryStream(f)), "file"));
        
        HttpRequestMessage httpRequestMessage = new() {
            Method = httpMethod,
            RequestUri = new Uri(uri),
            Content = multipartFormDataContent,
            Headers = {
                { HttpRequestHeader.Accept.ToString(), "application/json" }
            }
        };
        
        headers?.ToList().ForEach(h => httpRequestMessage.Headers.Add(h.Key, h.Value));

        using (HttpClient httpClient = new()) {
            HttpResponseMessage responseMessage = httpClient.Send(httpRequestMessage);

            BaseData responseData = new() {
                IsOk = responseMessage.IsSuccessStatusCode,
                Data = responseMessage.Content
            };

            return responseData;
        }
    }

    public static async Task DownloadFile(string uri, string path) {
        string decodedPath = HttpUtility.UrlDecode(path);
        
        if (File.Exists(decodedPath)) return;
        
        BaseData response = await JsonHttpRequest(uri, HttpMethod.Get, null, null);
        string fileName = Path.GetFileName(decodedPath);

        try {
            await using (FileStream fileStream = new(decodedPath, FileMode.CreateNew)) {
                await response.Data.CopyToAsync(fileStream);
                
#if DEBUG
                Logger.Debug($"Downloaded file {fileName} from {uri}");
#endif
            }
        } catch (Exception e) {
            Logger.Error($"Exception while downloading file {fileName} from {uri}: {e}");
        }
    }

    public static async Task<Stream> GetPictureAsStream(string uri) {
        BaseData response = await JsonHttpRequest(uri, HttpMethod.Get, null, null);

        return await response.Data.ReadAsStreamAsync();
    }
}
