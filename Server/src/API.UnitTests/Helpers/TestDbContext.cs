using API.Core.Entities;
using API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.UnitTests.Helpers;

public class TestDbContext : ApplicationDbContext
{
    public TestDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TokenEntity>()
            .Property(x => x.RefreshTokenExpireAt)
            .HasConversion(u => u.DateTime, 
                u => new DateTimeOffset(u));

        modelBuilder.Entity<RevokedTokenEntity>()
            .Property(x => x.TokenExpireAt)
            .HasConversion(u => u.DateTime,
                u => new DateTimeOffset(u));

        modelBuilder.Entity<RecoveryTokenEntity>()
            .Property(x => x.ExpireAt)
            .HasConversion(u => u.DateTime,
                u => new DateTimeOffset(u));
    }
}