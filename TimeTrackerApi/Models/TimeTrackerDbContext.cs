using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TimeTrackerApi.Models;

public class TimeTrackerDbContext : DbContext
{
    public TimeTrackerDbContext()
    {
    }

    public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<ActivityPeriod> ActivityPeriods { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectActivity> ProjectActivities { get; set; }
    public DbSet<ProjectUser> ProjectUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfiguration configuration = builder.Build();
            var connectionString = configuration["DbConnectionString"];

            //var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            //if (string.IsNullOrEmpty(connectionString))
            //{
            //    throw new InvalidOperationException("DbConnectionString is not set.");
            //}
            Console.WriteLine($"Trying to connect to DB with connection string: {connectionString}");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ChatId).HasMaxLength(100).HasDefaultValue(0);
            entity.HasIndex(e => e.ChatId).IsUnique();

            entity.HasMany(e => e.Activities)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.ActiveFrom).IsRequired().HasColumnType("timestamp without time zone"); ;
            entity.Property(e => e.StatusId).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(e => e.UserId);

            entity.HasMany(e => e.ActivityPeriods)
                .WithOne(a => a.Activity)
                .HasForeignKey(a => a.ActivityId);

            entity.HasOne(e => e.Status)
                .WithMany(y => y.Activities)
                .HasForeignKey(e => e.StatusId);
        });

        modelBuilder.Entity<ActivityPeriod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.StartTime).IsRequired().HasColumnType("timestamp without time zone");
            entity.Property(e => e.StopTime).HasColumnType("timestamp without time zone").IsRequired(false);
            entity.Property(e => e.ActivityId).IsRequired();
            entity.Property(e => e.TotalTime).IsRequired(false);
            entity.Property(e => e.TotalSeconds).IsRequired(false);

            entity.HasOne(e => e.Activity)
                .WithMany(a => a.ActivityPeriods)
                .HasForeignKey(e => e.ActivityId);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasData(
                new Status { Id = 1, Name = "Active" },
                new Status { Id = 2, Name = "Tracking" },
                new Status { Id = 3, Name = "Archived" }
            );

            entity.HasMany(e => e.Activities)
                .WithOne(a => a.Status)
                .HasForeignKey(e => e.StatusId);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name);

            entity.HasMany(e => e.ProjectUsers)
                  .WithOne(a => a.Project)
                  .HasForeignKey(e => e.ProjectId);

            entity.HasMany(e => e.ProjectActivities)
                  .WithOne(a => a.Project)
                  .HasForeignKey(e => e.ProjectId);

        });

        modelBuilder.Entity<ProjectUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UserId);
            entity.Property(e => e.ProjectId);
            entity.Property(e => e.Creator);

            entity.HasOne(e => e.Project)
                  .WithMany(a => a.ProjectUsers)
                  .HasForeignKey(e => e.ProjectId);

            entity.HasOne(e => e.User)
                  .WithMany(a => a.ProjectUsers)
                  .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<ProjectActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ActivityId);
            entity.Property(e => e.ProjectId);

            entity.HasOne(e => e.Project)
                  .WithMany(a => a.ProjectActivities)
                  .HasForeignKey(e => e.ProjectId);

            entity.HasOne(e => e.Activity)
                  .WithMany(a => a.ProjectActivities)
                  .HasForeignKey(e => e.ActivityId);
        });
    }
}
