using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using KuraSharp.Data;
using KuraSharp.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace KuraSharp.Windows; 

public partial class Chat : Window {
    static readonly ILogger Logger = Log.Logger.ForType<Chat>();
    static readonly UserData UserData = StaticData.UserData;

    public Chat() {
        InitializeComponent();
        ClientSize = StaticData.Size;
        
        FetchServers(true);

#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    async void FetchServers(bool addToPanel) {
        Dictionary<string, string> headers = new() {
            { "Authorization", UserData.Token }
        };

        await Parallel.ForEachAsync(UserData.Guilds, async (guild, cancel) => {
            BaseData response = await UrlExtensions.JsonHttpRequest($"https://api.kuracord.tk/guilds/{guild.Guild.Id}/", HttpMethod.Get, null, headers);
            string jsonResponse = await response.Data.ReadAsStringAsync(cancel);

            if (!response.IsOk) {
                ErrorData error = JsonConvert.DeserializeObject<ErrorData>(jsonResponse);
                Logger.Error(error.ToString());
                return;
            }

            GuildData guildData = JsonConvert.DeserializeObject<GuildData>(jsonResponse);
            StaticData.UserGuilds.Add(guildData);

            if (addToPanel) await Dispatcher.UIThread.InvokeAsync(() => AddServerToPanel(guildData));
        });
    }

    async void AddServerToPanel(GuildData guild) {
        Button server = new() {
            Name = guild.Name,
            Tag = guild.Id,
            Content = guild.ShortName,
            BorderThickness = new Thickness(0),
            Background = new SolidColorBrush(Colors.Transparent),
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            Width = 70, Height = 70,
            CornerRadius = new CornerRadius(50),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        if (guild.Icon != null) {
            Image serverIcon = new() {
                Source = new Bitmap(await UrlExtensions.GetPictureAsStream(guild.Icon)),
                Stretch = Stretch.Uniform
            };
            
            server.Content = serverIcon;
        }

        Servers.Children.Add(server);
    }

    async void Upload_OnClick(object? s, RoutedEventArgs e) {
        byte[] bytes = await File.ReadAllBytesAsync(@"C:\Users\Stm07\Desktop\nado\DitGu-hSxYM.jpg");
        string picture = Convert.ToBase64String(bytes);

        Dictionary<string, string> data = new() {
            { "name", "ХУЙ" },
            { "file", picture }
        };

        Dictionary<string, string> headers = new() {
            { "Authorization", UserData.Token }
        };

        await UrlExtensions.JsonHttpRequest("https://api.kuracord.tk/guilds/3/", HttpMethod.Patch, data, headers);
    }
}