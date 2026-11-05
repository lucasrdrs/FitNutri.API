using br.com.FitNutri.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace br.com.FitNutri.Api.Data;

public class ApplicationDbContext
    : IdentityDbContext<FitNutriUser, IdentityRole<int>, int> {
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<FitNutriUser>()
            .Property(user => user.Id)
            .UseIdentityByDefaultColumn();
    }

}
