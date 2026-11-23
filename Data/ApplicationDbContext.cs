using br.com.FitNutri.Api.Models;
using br.com.FitNutri.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace br.com.FitNutri.Api.Data;

public class ApplicationDbContext
    : IdentityDbContext<FitNutriUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<NutritionistProfile> NutritionistProfiles =>
        Set<NutritionistProfile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<FitNutriUser>()
            .Property(user => user.Id)
            .UseIdentityByDefaultColumn();

        builder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfiles");

            entity.HasKey(profile => profile.UserId);

            entity.Property(profile => profile.UserId)
                .ValueGeneratedNever();

            entity.HasOne(profile => profile.User)
                .WithOne(user => user.UserProfile)
                .HasForeignKey<UserProfile>(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<NutritionistProfile>(entity =>
        {
            entity.ToTable("NutritionistProfiles");

            entity.HasKey(profile => profile.UserId);

            entity.Property(profile => profile.UserId)
                .ValueGeneratedNever();

            entity.HasOne(profile => profile.User)
                .WithOne(user => user.NutritionistProfile)
                .HasForeignKey<NutritionistProfile>(
                    profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
