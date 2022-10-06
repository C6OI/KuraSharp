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
    
    public static async Task<HttpResponseMessage> GetRequest(Uri uri) {
        using (HttpClient httpClient = new())
            return await httpClient.GetAsync(uri);
    }

    public static BaseData HttpRequest(string uri, HttpMethod httpMethod,  Dictionary<string, string>? jsonData, Dictionary<string, string>? headers) {
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
            HttpResponseMessage responseMessage = httpClient.SendAsync(httpRequestMessage).Result;

            BaseData responseData = new() {
                Response = responseMessage.StatusCode,
                JsonData = responseMessage.Content.ReadAsStringAsync().Result
            };

            return responseData;
        }
    }

    public static async Task DownloadFile(Uri uri, string path) {
        string decodedPath = HttpUtility.UrlDecode(path);
        
        if (File.Exists(decodedPath)) return;
        
        HttpResponseMessage response = await GetRequest(uri);
        string fileName = Path.GetFileName(decodedPath);

        try {
            await using (FileStream fileStream = new(decodedPath, FileMode.CreateNew)) {
                await response.Content.CopyToAsync(fileStream);
                
#if DEBUG
                Logger.Debug($"Downloaded file {fileName} from {uri}");
#endif
            }
        } catch (Exception e) {
            Logger.Error($"Exception while downloading file {fileName} from {uri}: {e}");
        }
    }
}
