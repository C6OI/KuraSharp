using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using KuraSharp.Data;
using KuraSharp.Extensions;
using Serilog;
using Websocket.Client;

namespace KuraSharp; 

public partial class Chat : Window {
    static readonly ILogger Logger = Log.Logger.ForType<Chat>();
    static readonly UserData UserData = StaticData.UserData;
    static readonly WebsocketClient KuraGate = new(new Uri("wss://gateway.kuracord.tk/v3"));

    public Chat() {
        KuraGate.Start();
        
        InitializeComponent();
        ClientSize = StaticData.Size;

        FetchServers();

#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    void FetchServers() {
        UserData.Guilds.ForEach(AddServers);
    }

    async void AddServers(UserGuildInfo g) {
        Button server = new() {
            Name = g.Guild.Name,
            Tag = g.Guild.Id,
            BorderThickness = new Thickness(0),
            Background = new SolidColorBrush(Colors.Transparent),
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            Width = 70,
            Height = 70,
            CornerRadius = new CornerRadius(50),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        if (g.Guild.Icon != null) {
            string iconUri = $"https://cdn.kuracord.tk/icons/{g.Guild.Id}/{g.Guild.Icon}";
            Image serverIcon = new() { Source = new Bitmap(await UrlExtensions.GetPictureAsStream(iconUri)) };
            server.Content = serverIcon;
        } else {
            server.Content = g.Guild.Name;
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

        BaseData response = await UrlExtensions.JsonHttpRequest("https://api.kuracord.tk/guilds/3/", HttpMethod.Patch, data, headers);

        switch (response.Response) {
            case HttpStatusCode.OK:
                Logger.Information(response.Data.ReadAsStringAsync().Result);
                break;
            
            default:
                Logger.Error(response.Data.ReadAsStringAsync().Result);
                break;
        }
    }
}