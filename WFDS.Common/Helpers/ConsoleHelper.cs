using Microsoft.Extensions.Logging;
using ZLogger;

namespace WFDS.Common.Helpers;

public static class ConsoleHelper
{
    private static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(ConsoleHelper).FullName ?? nameof(ConsoleHelper));
    
    public static void UpdateConsoleTitle(string name, string code, int cur, int cap)
    {
        var title = $"[{cur}/{cap - 1}] {name} [{code}]";
        Console.Title = title;
        Logger.ZLogInformation($"update console title : {title}");
    }
}