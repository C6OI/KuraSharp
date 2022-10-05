using System.Net;

namespace KuraSharp.Data; 

public class BaseData {
    public HttpStatusCode Response { get; set; }
    public string JsonData { get; set; } = null!;
}
