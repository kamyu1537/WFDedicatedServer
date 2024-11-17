using System.Diagnostics;

namespace WFDS.Server.Core.Utils;

public static class SystemMonitor
{
    private static readonly bool True = true;
    public static double CpuUsage { get; private set; }
    
    public static double PrivateMemorySize { get; private set; }
    public static double VirtualMemorySize { get; private set; }
    public static double PagedMemorySize { get; private set; }
    public static double WorkingSet { get; private set; }
    public static double PeakWorkingSet { get; private set; }
    public static double PeakPagedMemorySize { get; private set; }
    public static double PeakVirtualMemorySize { get; private set; }
    
    public static double TotalHeapMemory { get; private set; }

    private static readonly Thread Thread;
    
    static SystemMonitor()
    {
        var process = Process.GetCurrentProcess();
        Thread = new Thread(() =>
        {
            while (True)
            {
                var startCpuUsage = process.TotalProcessorTime;
                var startTime = DateTimeOffset.UtcNow;

                Thread.Sleep(1000);
                
                var endCpuUsage = process.TotalProcessorTime;
                var endTime = DateTimeOffset.UtcNow;
                
                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;

                CpuUsage = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;
                
                PrivateMemorySize = process.PrivateMemorySize64;
                VirtualMemorySize = process.VirtualMemorySize64;
                PagedMemorySize = process.PagedMemorySize64;
                WorkingSet = process.WorkingSet64;
                PeakWorkingSet = process.PeakWorkingSet64;
                PeakPagedMemorySize = process.PeakPagedMemorySize64;
                PeakVirtualMemorySize = process.PeakVirtualMemorySize64;
                TotalHeapMemory = GC.GetTotalMemory(false);
            }
        });
    }
    
    public static void Start()
    {
        Thread.Start();
    }
}