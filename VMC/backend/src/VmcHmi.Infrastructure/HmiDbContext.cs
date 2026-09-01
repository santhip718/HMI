using Microsoft.EntityFrameworkCore;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Entities;

namespace VmcHmi.Infrastructure;

public class HmiDbContext : DbContext
{
    public HmiDbContext(DbContextOptions<HmiDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<MachineSession> MachineSessions => Set<MachineSession>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<OperationRun> OperationRuns => Set<OperationRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("hmi");

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Username).HasColumnName("username").IsRequired().HasMaxLength(100);
            builder.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            builder.Property(u => u.CreatedAt).HasColumnName("created_at");
            builder.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<MachineSession>(builder =>
        {
            builder.ToTable("machine_sessions");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.UserId).HasColumnName("user_id");
            builder.Property(s => s.CurrentStage).HasColumnName("current_stage").HasConversion<string>();
            builder.Property(s => s.OperationStatus).HasColumnName("operation_status").HasConversion<string>();
            builder.Property(s => s.CreatedAt).HasColumnName("created_at");
            builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
            builder.HasMany(s => s.ChecklistItems).WithOne(i => i.Session).HasForeignKey(i => i.SessionId);
            builder.HasMany(s => s.Tools).WithOne().HasForeignKey("SessionId");
            builder.HasOne(s => s.OperationRun).WithOne(o => o.Session).HasForeignKey<OperationRun>(o => o.SessionId);
        });

        modelBuilder.Entity<ChecklistItem>(builder =>
        {
            builder.ToTable("checklist_items");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.SessionId).HasColumnName("session_id");
            builder.Property(i => i.Stage).HasColumnName("stage").HasConversion<string>();
            builder.Property(i => i.Label).HasColumnName("label").IsRequired();
            builder.Property(i => i.SortOrder).HasColumnName("sort_order");
            builder.Property(i => i.IsConfirmed).HasColumnName("is_confirmed");
            builder.Property(i => i.ConfirmedAt).HasColumnName("confirmed_at");
        });

        modelBuilder.Entity<Tool>(builder =>
        {
            builder.ToTable("tools");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
            builder.Property(t => t.Description).HasColumnName("description").IsRequired();
        });

        modelBuilder.Entity<OperationRun>(builder =>
        {
            builder.ToTable("operation_runs");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.SessionId).HasColumnName("session_id");
            builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>();
            builder.Property(o => o.StartedAt).HasColumnName("started_at");
            builder.Property(o => o.StoppedAt).HasColumnName("stopped_at");
        });
    }
}
