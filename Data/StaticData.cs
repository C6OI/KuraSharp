using Avalonia;

namespace KuraSharp.Data;

public static class StaticData {
    public static UserData UserData { get; set; } = new();
    public static Size Size { get; set; } = new(960, 540);
}
