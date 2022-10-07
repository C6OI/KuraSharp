using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KuraSharp.Data;
using KuraSharp.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using Newtonsoft.Json;
using Serilog;

namespace KuraSharp; 

public partial class Entrance : Window {
    static readonly ILogger Logger = Log.Logger.ForType<Entrance>();
    readonly MessageBoxStandardParams _mBoxParams;
    bool _authHandled;
    
    public Entrance() {
        _mBoxParams = new MessageBoxStandardParams {
            WindowIcon = Icon,
            Icon = MessageBox.Avalonia.Enums.Icon.Error,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        
        InitializeComponent();
        ClientSize = new Size(960, 540);
    }

    void OnKeyDown(object? s, KeyEventArgs e) {
        if (e.Key is not (Key.Enter or Key.Return)) return;
        
        if (RegisterPanel.IsVisible) RegisterButton_OnClick(this, new RoutedEventArgs());
        else LoginButton_OnClick(this, new RoutedEventArgs());
    }

    async void RegisterButton_OnClick(object? s, RoutedEventArgs e) {
        if (_authHandled) return;
        _authHandled = true;
        
        if (RegisterUsernameField?.Text is null or "" ||
            RegisterPasswordField?.Text is null or "" ||
            RegisterEmailField?.Text is null or "") {
            Logger.Error("Registration error: Username/Password/Email field is null or empty");

            _mBoxParams.ContentTitle = "Ошибка";
            _mBoxParams.ContentMessage = "Заполните все поля";
            await MessageBoxManager.GetMessageBoxStandardWindow(_mBoxParams).ShowDialog(this);
            
            _authHandled = false;
            return;
        }
        
        Dictionary<string, string> data = new() {
            { "username", RegisterUsernameField.Text },
            { "email", RegisterEmailField.Text },
            { "password", RegisterPasswordField.Text },
            { "token", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
        };
            
        BaseData response = await UrlExtensions.JsonHttpRequest("https://api.kuracord.tk/users/", HttpMethod.Post, data, null);

        switch (response.Response) {
            case HttpStatusCode.OK:
                StaticData.UserData = JsonConvert.DeserializeObject<UserData>(await response.Data.ReadAsStringAsync());
                Logger.Information($"Registration successful. Account: {StaticData.UserData.User()}");
                ShowChat();
                break;
            
            default: {
                HttpError.Error(response, this);
                _authHandled = false;
                break;
            }
        }
    }
    
    async void LoginButton_OnClick(object? s, RoutedEventArgs e) {
        if (_authHandled) return;
        _authHandled = true;

        if (LoginEmailField?.Text is null or "" ||
            LoginPasswordField?.Text is null or "") {
            Logger.Error("Login error: Username/Password field is null or empty");

            _mBoxParams.ContentTitle = "Ошибка";
            _mBoxParams.ContentMessage = "Заполните все поля";
            await MessageBoxManager.GetMessageBoxStandardWindow(_mBoxParams).ShowDialog(this);

            _authHandled = false;
            return;
        }

        Dictionary<string, string> loginData = new() {
            { "email", LoginEmailField.Text },
            { "password", LoginPasswordField.Text }
        };

        BaseData loginResponse = await UrlExtensions.JsonHttpRequest("https://api.kuracord.tk/users/login", HttpMethod.Post, loginData, null);

        switch (loginResponse.Response) {
            case HttpStatusCode.OK: {
                StaticData.UserData = JsonConvert.DeserializeObject<UserData>(await loginResponse.Data.ReadAsStringAsync());
                
                Dictionary<string, string> userData = new() {
                    { "Authorization", StaticData.UserData.Token }
                };

                BaseData dataResponse = await UrlExtensions.JsonHttpRequest("https://api.kuracord.tk/users/@me/", HttpMethod.Get, null, userData);

                switch (dataResponse.Response) {
                    case HttpStatusCode.OK: {
                        StaticData.UserData = JsonConvert.DeserializeObject<UserData>(await dataResponse.Data.ReadAsStringAsync());
                        Logger.Information($"Login successful. Account: {StaticData.UserData.User()}");
                        ShowChat();
                        break;
                    }
                    
                    default:
                        HttpError.Error(dataResponse, this);
                        _authHandled = false;
                        break;
                }
                
                break;
            }

            default:
                HttpError.Error(loginResponse, this);
                _authHandled = false;
                break;
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

    void ShowChat() {
        StaticData.Size = ClientSize;
        new Chat().Show();
        Close();
    }
}