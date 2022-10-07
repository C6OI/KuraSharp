using System.Net.Http;

namespace KuraSharp.Data; 

public class BaseData {
    public bool IsOk { get; set; }
    public HttpContent Data { get; set; } = null!;
}
