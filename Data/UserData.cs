using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KuraSharp.Data;

public class UserGuildInfo {
    public string? Nickname { get; set; }
    public int Id { get; set; }
    public DateTime JoinedAt { get; set; }
    public GuildInfo Guild { get; set; } = new();
}

public class GuildInfo {
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string VanityUrl { get; set; } = "";
    public string? Description { get; set; }
    public int Id { get; set; }
    public bool Disabled { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string? Icon {
        get => _icon;
        set {
            if (value != null) _icon = $"https://cdn.kuracord.tk/icons/{Id}/{value}";
        }
    }

    [JsonIgnore] string? _icon;
}

public class UserData : BaseData {
    public string Username { get; set; } = "";
    public string Discriminator { get; set; } = "0000";
    public string Email { get; set; } = "";
    public string Token { get; set; } = "";
    public string Avatar { get; set; } = "";
    public string Bio { get; set; } = "";
    public int Id { get; set; }
    public int Flags { get; set; }
    public int PremiumType { get; set; }
    public bool Disabled { get; set; }
    public bool Bot { get; set; }
    public bool Verified { get; set; }
    public List<UserGuildInfo> Guilds { get; set; } = new();

    public string User() => $"{Username}#{Discriminator}";
}