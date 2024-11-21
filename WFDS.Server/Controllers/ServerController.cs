using Microsoft.AspNetCore.Mvc;
using WFDS.Server.Core.Utils;

namespace WFDS.Server.Controllers;

[ApiController]
[Tags("server")]
[Route("api/v1/server")]
public class ServerController : Controller
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Json(new
        {
            SystemMonitor.CpuUsage,
            PrivateMemorySizeMB = SystemMonitor.PrivateMemorySize / (1024 * 1024),
            VirtualMemorySizeMB = SystemMonitor.VirtualMemorySize / (1024 * 1024),
            PagedMemorySizeMB = SystemMonitor.PagedMemorySize / (1024 * 1024),
            WorkingSetMB = SystemMonitor.WorkingSet / (1024 * 1024),
            PeakWorkingSetMB = SystemMonitor.PeakWorkingSet / (1024 * 1024),
            PeakPagedMemorySizeMB = SystemMonitor.PeakPagedMemorySize / (1024 * 1024),
            PeakVirtualMemorySizeMB = SystemMonitor.PeakVirtualMemorySize / (1024 * 1024),
            TotalHeapMemoryMB = SystemMonitor.TotalHeapMemory / (1024 * 1024)
        });
    }
}