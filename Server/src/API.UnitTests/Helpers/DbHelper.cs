using API.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace API.UnitTests.Helpers;

public class DbHelper
{

    public static ApplicationDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseInMemoryDatabase(databaseName: "ApplicationDbContextInMemory");

        var dbContextOptions = builder.Options;
        var dbContext = new ApplicationDbContext(dbContextOptions);
        
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        dbContext.SaveChanges();
        
        return dbContext;
    }
    
    public static TestDbContext CreateSqLiteDbContext()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseSqlite(new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = ":memory:" }.ToString()));

        var dbContextOptions = builder.Options;
        var dbContext = new TestDbContext(dbContextOptions);
        
        dbContext.Database.OpenConnection();
        dbContext.Database.EnsureCreated();
        dbContext.SaveChanges();
        
        return dbContext;
    }

    //todo helpers add user, role
}