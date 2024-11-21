using Serilog;

namespace WFDS.Common.Helpers;

public static class ConsoleHelper
{
    public static void UpdateConsoleTitle(string name, string code, int cur, int cap)
    {
        var title = $"[{cur}/{cap - 1}] {name} [{code}]";
        Console.Title = title;
        Log.Logger.Information("update console title : {0}", title);
    }
}