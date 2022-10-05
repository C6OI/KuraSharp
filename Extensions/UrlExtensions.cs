using System;
using System.Collections.Generic;
using System.IO;
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

    public static string PostRequest(string uri, IEnumerable<KeyValuePair<string, string>> data) {
        using (HttpClient httpClient = new()) {
            HttpContent content = new FormUrlEncodedContent(data);
            HttpResponseMessage responseMessage = httpClient.PostAsync(uri, content).Result;

            Logger.Information(responseMessage.StatusCode.ToString());
            
            return responseMessage.Content.ReadAsStringAsync().Result;
        }
    }

    public static BaseData PostRequest(string uri, Dictionary<string, string> data, string contentType = "application/json") {
        string serializedData = JsonConvert.SerializeObject(data);

        using (HttpClient httpClient = new()) {
            HttpContent content = new StringContent(serializedData, Encoding.UTF8, contentType);
            HttpResponseMessage responseMessage = httpClient.PostAsync(uri, content).Result;

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
