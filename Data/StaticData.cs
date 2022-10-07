using System.Collections.Generic;
using Avalonia;

namespace KuraSharp.Data;

public static class StaticData {
    public static List<GuildData> UserGuilds { get; set; } = new();
    public static UserData UserData { get; set; } = new();
    public static Size Size { get; set; } = new(960, 540);
}
