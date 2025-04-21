using Microsoft.EntityFrameworkCore;
using server.Classes;

namespace server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Issue> Issues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Issue>(entity =>
        {
            entity.ToTable("issues");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerEmail).IsRequired();
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Subject).IsRequired();
            entity.Property(e => e.Created).IsRequired();
            entity.Property(e => e.Latest).IsRequired();
            entity.Property(e => e.State).IsRequired();
        });
    }
} 