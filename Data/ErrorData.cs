using System.Text.Json.Serialization;

namespace KuraSharp.Data; 

public class ErrorData : BaseData {
    [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
    [JsonPropertyName("error")] public string Error { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; }
}

