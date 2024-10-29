using CrushBot.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CrushBot.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BotUser> Users { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<CityName> CityNames { get; set; }
    public DbSet<UserFilter> Filters { get; set; }
    public DbSet<UserLike> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BotUser>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<BotUser>()
            .HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BotUser>()
            .HasOne(x => x.Filter)
            .WithOne(x => x.User)
            .HasForeignKey<UserFilter>(x => x.UserId)
            .IsRequired();

        modelBuilder.Entity<BotUser>()
            .HasMany(x => x.Likes)
            .WithOne()
            .HasForeignKey(x => x.LikerUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BotUser>()
            .HasMany(user => user.LikedBy)
            .WithOne()
            .HasForeignKey(x => x.LikedUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserLike>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<BotUser>()
            .Property(x => x.PhotoIds)
            .HasConversion(
                x => string.Join(';', x),
                x => x.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                new ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    x => x.Aggregate(0, (hash, item) =>
                        HashCode.Combine(hash, item.GetHashCode())),
                    x => x.ToList()));

        modelBuilder.Entity<City>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<City>()
            .HasMany(x => x.CityNames)
            .WithOne(x => x.City)
            .HasForeignKey(x => x.CityId)
            .IsRequired();

        modelBuilder.Entity<CityName>()
            .HasKey(x => new { x.CityId, x.Language });
    }
}