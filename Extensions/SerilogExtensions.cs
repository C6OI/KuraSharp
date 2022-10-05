using System;
using Serilog;
using Serilog.Core;

namespace KuraSharp.Extensions; 

public static class SerilogExtensions {
    public static ILogger ForType<T>(this ILogger logger) => logger.ForContext(Constants.SourceContextPropertyName, typeof(T).Name);
    public static ILogger ForType(this ILogger logger, Type type) => logger.ForContext(Constants.SourceContextPropertyName, type.Name);
}