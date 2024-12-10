using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WFDS.Database;

public class DataDbContextFactory : IDesignTimeDbContextFactory<DataDbContext>
{
    public DataDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataDbContext>();
        optionsBuilder.UseSqlite("Data Source=./data.db;Cache=Shared");
        
        return new DataDbContext(optionsBuilder.Options);
    }
}