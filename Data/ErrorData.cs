namespace KuraSharp.Data; 

public class ErrorData : BaseData {
    public string Error { get; set; } = "";
    public string Message { get; set; } = "";
    public int StatusCode { get; set; }

    public override string ToString() => $"{Error} ({StatusCode}):\n{Message}";
}

