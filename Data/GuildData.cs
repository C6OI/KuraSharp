using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KuraSharp.Data;

public class Channel : BaseData {
    public string Name { get; set; } = "";
    public int Id { get; set; }
    public int Type { get; set; }
}

public class Member {
    public string? Nickname { get; set; }
    public int Id { get; set; }
    public DateTime JoinedAt { get; set; }
    public User User { get; set; } = new();
}

public class Owner {
    public string Username { get; set; } = "";
    public string Discriminator { get; set; } = "";
    public string Avatar { get; set; } = "";
    public string Bio { get; set; } = "";
    public int Id { get; set; }
    public int Flags { get; set; }
}

public class Role {
    public string Name { get; set; } = "";
    public int Id { get; set; }
    public int Color { get; set; }
    public int Permissions { get; set; }
    public bool Hoist { get; set; }
}

public class GuildData : BaseData {
    public string Name { get; set; } = "";
    public string VanityUrl { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string? Description { get; set; }
    public int Id { get; set; }
    public bool Disabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public Owner Owner { get; set; } = new();
    public List<Member> Members { get; set; } = new();
    public List<Channel> Channels { get; set; } = new();
    public List<Role> Roles { get; set; } = new();

    public string? Icon {
        get => _icon;
        set {
            if (value != null) _icon = $"https://cdn.kuracord.tk/icons/{Id}/{value}";
        }
    }

    string? _icon;
    [JsonIgnore] public bool Opened { get; set; }
}

public class User {
    public string Username { get; set; } = "";
    public string Discriminator { get; set; } = "";
    public string Avatar { get; set; } = "";
    public string Bio { get; set; } = "";
    public int Id { get; set; }
    public int Flags { get; set; }
}