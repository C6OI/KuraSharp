using System.Collections.Generic;
using KuraSharp.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates.Themes;

namespace KuraSharp.Services;

public interface ILoggerService {
    public void Init();
}

[Service]
public class LoggerService : ILoggerService {
    static readonly ILogger Logger = Log.Logger.ForType<LoggerService>();

    static readonly TemplateTheme Theme = new(new Dictionary<TemplateThemeStyle, string> {
        [TemplateThemeStyle.Text] = "\u001B[38;5;0253m",
        [TemplateThemeStyle.SecondaryText] = "\u001B[38;5;0246m",
        [TemplateThemeStyle.TertiaryText] = "\u001B[38;5;0242m",
        [TemplateThemeStyle.Invalid] = "\u001B[33;1m",
        [TemplateThemeStyle.Null] = "\u001B[38;5;0038m",
        [TemplateThemeStyle.Name] = "\u001B[38;5;0081m",
        [TemplateThemeStyle.String] = "\u001B[38;5;0216m",
        [TemplateThemeStyle.Number] = "\u001B[38;5;151m",
        [TemplateThemeStyle.Boolean] = "\u001B[38;5;0038m",
        [TemplateThemeStyle.Scalar] = "\u001B[38;5;0079m",
        [TemplateThemeStyle.LevelVerbose] = "\u001B[34m",
        [TemplateThemeStyle.LevelDebug] = "\u001b[36m",
        [TemplateThemeStyle.LevelInformation] = "\u001B[32m",
        [TemplateThemeStyle.LevelWarning] = "\u001B[33;1m",
        [TemplateThemeStyle.LevelError] = "\u001B[31;1m",
        [TemplateThemeStyle.LevelFatal] = "\u001B[31;1m"
    });

    public LogEventLevel MinimumLevel {
        get => _level.MinimumLevel;
        set => _level.MinimumLevel = value;
    }

    readonly LoggingLevelSwitch _level;

    public LoggerService() {
        _level = new LoggingLevelSwitch {
            MinimumLevel = LogEventLevel.Verbose
        };
    }

    public void Init() {
        LoggerConfiguration config = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(_level)
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.FromLogContext()
            .WriteTo.File(
                "Logs/KuracordClient-.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 1048576
            );

        Log.Logger = config.CreateLogger();

        Logger.Information("Initialized");
    }
}
