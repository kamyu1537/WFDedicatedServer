using Microsoft.AspNetCore.Mvc.RazorPages;
using WFDS.Database;
using WFDS.Database.DbSet;

namespace WFDS.Server.Pages.Chat;

public class Index(DatabaseContext dbContext) : PageModel
{
    public void OnGet()
    {
        
    }

    public IEnumerable<ChatHistory> ChatHistories()
    {
        return dbContext.ChatHistories.OrderByDescending(x => x.Id).Take(100);
    }
}