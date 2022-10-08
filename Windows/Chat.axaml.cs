using System.Collections.Generic;
using System.Linq;
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
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.DTO;
using Newtonsoft.Json;
using Serilog;

namespace KuraSharp.Windows; 

public partial class Chat : Window {
    static readonly ILogger Logger = Log.Logger.ForType<Chat>();
    static readonly UserData UserData = StaticData.UserData!;
    readonly Dictionary<string, string>? _authHeader;

    public Chat() {
        InitializeComponent();
        ClientSize = StaticData.Size;
        
        // Don't remove it! Previewer doesn't work without it
        if (StaticData.UserData == null) return;

        _authHeader = new Dictionary<string, string> { { "Authorization", UserData.Token } };
        
        SetupProfile();

#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    async void SetupProfile() {
        Avatar.Source = new Bitmap(await UrlExtensions.GetPictureAsStream(UserData.Avatar));
        Username.Text = $"{UserData.Username}\n#{UserData.Discriminator}";

        FetchServers(true);
    }
    
    async void FetchServers(bool addToPanel) {
        await Parallel.ForEachAsync(UserData.Guilds, async (guild, cancel) => {
            BaseData response = await UrlExtensions.JsonHttpRequest($"https://api.kuracord.tk/guilds/{guild.Guild.Id}/", HttpMethod.Get, null, _authHeader);
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

    void OpenServer(GuildData server) {
        if (server.Opened) return;

        StaticData.UserGuilds.ForEach(g => {
            g.Opened = false;
            NewChannel.Click -= CreateChannelUserInput;
        });
        
        server.Opened = true;
        
        Channels.Children.Clear();
        server.Channels.ForEach(AddChannelToPanel);
        ServerInfo.Header = server.Name;
        
        NewChannel.Click += CreateChannelUserInput;
        Logger.Verbose($"Opened server {server.Name}");
        OpenChannel(server.Channels.FirstOrDefault());
    }
    
    void OpenChannel(Channel? channel) {
        StaticData.UserGuilds
            .ForEach(s => s.Channels
                .Where(c => c != channel)
                .ToList()
                .ForEach(c => c.Opened = false));
        
        ChannelName.Text = $"#{channel?.Name}";
        if (channel == null || channel.Opened) return;
        
        channel.Opened = true;

        Logger.Verbose($"Opened channel {channel.Name}");
    }

    async void CreateServer(object? s, RoutedEventArgs e) {
        string? name = await UserInput("Имя сервера");
        if (name == null) return;
        
        Dictionary<string, string> json = new() { { "name", name } };
        BaseData response = await UrlExtensions.JsonHttpRequest($"https://api.kuracord.tk/guilds/", HttpMethod.Post, json, _authHeader);

        if (!response.IsOk) {
            ErrorData error = JsonConvert.DeserializeObject<ErrorData>(await response.Data.ReadAsStringAsync());
            Logger.Error(error.ToString());
            return;
        }

        GuildData server = JsonConvert.DeserializeObject<GuildData>(await response.Data.ReadAsStringAsync());
        StaticData.UserGuilds.Add(server);
        AddServerToPanel(server);
        OpenServer(server);
        await CreateChannel(server, "general");
    }
    
    async void CreateChannelUserInput(object? s, RoutedEventArgs e) {
        GuildData server = StaticData.UserGuilds.First(g => g.Opened);

        string? name = await UserInput("Имя канала");
        if (name == null) return;
        
        Channel? channel = await CreateChannel(server, name);
        if (channel == null) return;
        
        OpenChannel(channel);
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
            VerticalContentAlignment = VerticalAlignment.Center,
            ClickMode = ClickMode.Release
        };

        if (guild.Icon != null) {
            Image serverIcon = new() {
                Source = new Bitmap(await UrlExtensions.GetPictureAsStream(guild.Icon))
            };
            
            server.Content = serverIcon;
        }

        server.Click += (_, _) => {
            OpenServer(guild);
        };
        
        Servers.Children.Insert(Servers.Children.Count - 1, server);
    }

    void AddChannelToPanel(Channel channel) {
        Button channelButton = new() {
            Name = channel.Name,
            Tag = channel.Id,
            Content = $"# {channel.Name}",
            BorderThickness = new Thickness(0),
            Background = new SolidColorBrush(Colors.Transparent),
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            HorizontalAlignment = HorizontalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            ClickMode = ClickMode.Release
        };

        channelButton.Click += (_, _) => OpenChannel(channel);
        Channels.Children.Add(channelButton);
    }

    async Task<string?> UserInput(string watermark) {
        MessageBoxInputParams mBoxInput = new() {
            WindowIcon = Icon,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Width = 480, Height = 240,
            Icon = MessageBox.Avalonia.Enums.Icon.Plus,
            Multiline = false,
            WatermarkText = watermark
        };
        
        MessageWindowResultDTO messageWindowResultDto =
            await MessageBoxManager.GetMessageBoxInputWindow(mBoxInput).ShowDialog(this);

        return messageWindowResultDto.Button == "Confirm" ? messageWindowResultDto.Message : null;
    }

    async Task<Channel?> CreateChannel(GuildData server, string name) {
        Dictionary<string, string> json = new() { { "name", name } };
        BaseData response = await UrlExtensions.JsonHttpRequest($"https://api.kuracord.tk/guilds/{server.Id}/channels", HttpMethod.Post, json, _authHeader);

        if (!response.IsOk) {
            ErrorData error = JsonConvert.DeserializeObject<ErrorData>(await response.Data.ReadAsStringAsync());
            Logger.Error(error.ToString());
            return null;
        }

        Channel channel = JsonConvert.DeserializeObject<Channel>(await response.Data.ReadAsStringAsync());
        server.Channels.Add(channel);
        AddChannelToPanel(channel);
        OpenChannel(channel);
        return channel;
    }
}