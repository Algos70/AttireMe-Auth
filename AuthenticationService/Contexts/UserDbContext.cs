using AuthenticationService.DTOs;
using AuthenticationService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Contexts;

public class UserDbContext(DbContextOptions<UserDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Creator> Creators { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<User>().ToTable("Users");
        builder.Entity<User>()
            .HasOne(u => u.Creator)
            .WithOne(v => v.User)
            .HasForeignKey<Creator>(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<User>()
            .HasOne(u => u.AppUser)
            .WithOne(c => c.User)
            .HasForeignKey<AppUser>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RefreshToken>().ToTable("RefreshTokens");
        SeedRoles(builder);
    }

    private void SeedRoles(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "ADMIN" },
            new IdentityRole() { Name = "Creator", ConcurrencyStamp = "2", NormalizedName = "CREATOR" },
            new IdentityRole() { Name = "User", ConcurrencyStamp = "3", NormalizedName = "USER" });
    }
}