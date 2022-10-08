using Avalonia.Controls;
using KuraSharp.Data;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Newtonsoft.Json;
using Serilog;

namespace KuraSharp.Extensions; 

public static class HttpError {
    static readonly ILogger Logger = Log.Logger.ForType(typeof(HttpError));
    
    public static async void Error(BaseData data, Window window) {
        MessageBoxStandardParams mBoxParams = new() {
            WindowIcon = window.Icon,
            Icon = Icon.Error,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        
        ErrorData errorData = JsonConvert.DeserializeObject<ErrorData>(await data.Data.ReadAsStringAsync());
                
        mBoxParams.ContentTitle = $"{errorData.StatusCode}: {errorData.Error}";
        mBoxParams.ContentMessage = errorData.Message;
        await MessageBoxManager.GetMessageBoxStandardWindow(mBoxParams).ShowDialog(window);
                
        Logger.Error(errorData.ToString());
    } 
}
