using System.Net;
using System.Net.Http;

namespace KuraSharp.Data; 

public class BaseData {
    public HttpStatusCode Response { get; set; }
    public HttpContent Data { get; set; } = null!;
}
