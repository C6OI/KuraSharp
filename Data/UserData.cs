using System.Text.Json.Serialization;

namespace KuraSharp.Data; 

public class UserData : BaseData {
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; }
    [JsonPropertyName("discriminator")] public string Discriminator { get; set; }
    [JsonPropertyName("flags")] public int Flags { get; set; }
    [JsonPropertyName("avatar")] public string Avatar { get; set; }
    [JsonPropertyName("bio")] public string Bio { get; set; }
    [JsonPropertyName("token")] public string Token { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("verified")] public bool Verified { get; set; }
    [JsonPropertyName("disabled")] public bool Disabled { get; set; }
    [JsonPropertyName("premiumType")]  public int PremiumType { get; set; }
}