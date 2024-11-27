using System.Globalization;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

namespace WFDS.Common;

public static class Log
{
    public static readonly ILoggerFactory Factory = LoggerFactory.Create(logging =>
    {
#if DEBUG
        logging.SetMinimumLevel(LogLevel.Debug);
#else
        logging.SetMinimumLevel(LogLevel.Information);
#endif

        logging.AddZLoggerConsole(options =>
        {
            options.IncludeScopes = true;
            options.UsePlainTextFormatter(formatter =>
            {
                formatter.SetPrefixFormatter($"[{0}] [{1:short}] [{2}] [{3}:{4}] ", (in MessageTemplate template, in LogInfo info) => template.Format(info.Timestamp.Utc.ToString("O"), info.LogLevel, info.Category, info.MemberName, info.LineNumber));
                formatter.SetExceptionFormatter((writer, ex) => Utf8StringInterpolation.Utf8String.Format(writer, $"{ex.Message}"));
            });
        });
        
        logging.AddZLoggerFile("Logs/latest.log", options =>
        {
            options.UseJsonFormatter();
        });
        logging.AddZLoggerRollingFile(options =>
        {
            options.FilePathSelector = (DateTimeOffset timestamp, int sequenceNumber) => $"logs/{timestamp.ToLocalTime():yyyyMMdd}_{sequenceNumber:000}.log";
            options.RollingInterval = RollingInterval.Day;
            options.RollingSizeKB = 1024;
        });
    });

    public static readonly ILogger Logger = Factory.CreateLogger("");
}