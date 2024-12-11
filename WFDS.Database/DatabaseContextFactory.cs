using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WFDS.Database;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseSqlite("Data Source=./data.db;Cache=Shared");
        
        return new DatabaseContext(optionsBuilder.Options);
    }
}