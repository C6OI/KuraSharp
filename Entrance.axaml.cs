using System;
using System.Collections.Generic;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KuraSharp.Data;
using KuraSharp.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace KuraSharp; 

public partial class Entrance : Window {
    static readonly ILogger Logger = Log.Logger.ForType<Entrance>();
    
    public Entrance() {
        InitializeComponent();
        ClientSize = new Size(960, 540);
    }

    void RegisterButton_OnClick(object? s, RoutedEventArgs e) {
        Dictionary<string, string> data = new() {
            { "username", RegisterUsernameField.Text },
            { "email", RegisterEmailField.Text },
            { "password", RegisterPasswordField.Text },
            { "token", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
        };
        
        BaseData response = UrlExtensions.PostRequest("https://api.kuracord.tk/users/", data);
        ErrorData errorData;

        switch (response.Response) {
            case HttpStatusCode.OK:
                StaticData.UserData = JsonConvert.DeserializeObject<UserData>(response.JsonData);
                Logger.Verbose($"ЛОЛ ты крутой: {StaticData.UserData.Username}");
                break;
            default: {
                errorData = JsonConvert.DeserializeObject<ErrorData>(response.JsonData);
                Logger.Verbose($"ЛОШАРА ты нуб: {errorData.Message}");
                break;
            }
        }
    }

    void AlreadyRegistered_OnClick(object? s, RoutedEventArgs e) {
        LoginPanel.IsVisible = true;
        RegisterPanel.IsVisible = false;
    }

    void NotRegistered_OnClick(object? s, RoutedEventArgs e) {
        RegisterPanel.IsVisible = true;
        LoginPanel.IsVisible = false;
    }
}