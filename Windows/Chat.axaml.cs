using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using KuraSharp.Data;
using KuraSharp.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Views;
using Newtonsoft.Json;
using Serilog;

namespace KuraSharp.Windows; 

public partial class Chat : Window {
    static readonly ILogger Logger = Log.Logger.ForType<Chat>();
    static readonly UserData UserData = StaticData.UserData!;
    Dictionary<string, string> _headers = new() { { "Authorization", UserData.Token } };

    public Chat() {
        InitializeComponent();
        ClientSize = StaticData.Size;
        
        // Don't remove it! Previewer doesn't work without it
        if (StaticData.UserData == null) return;
        
        SetupProfile();

#if DEBUG
        this.AttachDevTools();
#endif
    }

    async void SetupProfile() {
        Avatar.Source = new Bitmap(await UrlExtensions.GetPictureAsStream(UserData.Avatar));
        Username.Text = $"{UserData.Username}#{UserData.Discriminator}";

        FetchServers(true);
    }

    async void OpenChannel(Channel channel) {
        Logger.Information($"Trying to open channel {channel.Name}");
    }

    async void OpenServer(GuildData server) {
        ServerInfo.Header = server.Name;
        CreateChannel.Click += (s, e) => CreateChannelClick(s, e, server);
    }
    
    async void FetchServers(bool addToPanel) {
        await Parallel.ForEachAsync(UserData.Guilds, async (guild, cancel) => {
            BaseData response = await UrlExtensions.JsonHttpRequest($"https://api.kuracord.tk/guilds/{guild.Guild.Id}/", HttpMethod.Get, null, _headers);
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
            VerticalContentAlignment = VerticalAlignment.Center,
            ClickMode = ClickMode.Release
        };

        if (guild.Icon != null) {
            Image serverIcon = new() {
                Source = new Bitmap(await UrlExtensions.GetPictureAsStream(guild.Icon))
            };
            
            server.Content = serverIcon;
        }

        server.Click += (s, e) => {
            CreateChannel.Click -= (se, ev) => CreateChannelClick(se, ev, guild); 
            OpenServer(guild);
        };

        Servers.Children.Add(server);
    }
    
    async void CreateChannelClick(object? s, RoutedEventArgs e, GuildData server) {
        Dictionary<string, string> json = new();
        MessageBoxInputParams mBoxInput = new() {
            WindowIcon = Icon,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Width = 480, Height = 240,
            Icon = MessageBox.Avalonia.Enums.Icon.Plus,
            Multiline = false
        };
        
        IMsBoxWindow<MessageWindowResultDTO> messageBoxInputWindow = MessageBoxManager.GetMessageBoxInputWindow(mBoxInput);
        MessageWindowResultDTO messageWindowResultDto = await messageBoxInputWindow.ShowDialog(this);

        if (messageWindowResultDto.Button != "Confirm") return;

        json.Add("name", messageWindowResultDto.Message);
        BaseData response = await UrlExtensions.JsonHttpRequest($"https://api.kuracord.tk/guilds/{server.Id}/channels", HttpMethod.Post, json, _headers);

        if (!response.IsOk) {
            ErrorData error = JsonConvert.DeserializeObject<ErrorData>(await response.Data.ReadAsStringAsync());
            Logger.Error(error.Message);
            return;
        }

        Channel channel = JsonConvert.DeserializeObject<Channel>(await response.Data.ReadAsStringAsync());
        Button channelButton = new() {
            Name = channel.Name,
            Tag = channel.Id,
            Content = channel.Name,
            BorderThickness = new Thickness(0),
            Background = new SolidColorBrush(Colors.Transparent),
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            ClickMode = ClickMode.Release
        };

        channelButton.Click += (se, ev) => OpenChannel(channel);

        Channels.Children.Add(channelButton);

        Logger.Information($"Created channel {channel.Name}");
    }
}