using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WFDS.Common;

public static class Log
{
    private const string LogOutputTemplate = "[{Timestamp:O}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    private static readonly Lazy<Serilog.ILogger> SerilogLogger = new(() => new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        .WriteTo.Console(outputTemplate: LogOutputTemplate, theme: AnsiConsoleTheme.Code)
        .WriteTo.File(
            new JsonFormatter(),
            path: "Logs/log-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 5,
            fileSizeLimitBytes: 256 * 1024 * 1024,
            rollOnFileSizeLimit: true
        )
        .CreateLogger());

    public static ILoggerFactory Factory { get; } = new SerilogLoggerFactory(SerilogLogger.Value, dispose: true);

    public static ILogger Logger { get; } = Factory.CreateLogger(string.Empty);
}