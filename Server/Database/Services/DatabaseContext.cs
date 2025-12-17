using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Models;
using Server.Models.Users;
using Server.Models.UserPermissions;
using Server.Models.Events;

namespace Server.Database.Services
{
    public class DatabaseContext : DbContext, IDatabaseContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public override DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        // User-related DbSets
        public DbSet<User> Users { get; set; }

        // Authorization-related DbSets
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Event-related DbSets
        public DbSet<EventTypesModel> EventTypes { get; set; }
        public DbSet<EventsModel> Events { get; set; }
        public DbSet<UserEventsModel> UserEvents { get; set; }
        public DbSet<EventSettingsModel> EventSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserRole many-to-many relationship
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => ur.Id); // Use Id as primary key, not composite

            modelBuilder.Entity<UserRole>()
                .Property(ur => ur.Id)
                .ValueGeneratedOnAdd(); // Use database sequence to generate Id

            // Add unique constraint for UserId + RoleId to prevent duplicates
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure RolePermission many-to-many relationship
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => rp.Id); // Use Id as primary key, not composite

            modelBuilder.Entity<RolePermission>()
                .Property(rp => rp.Id)
                .ValueGeneratedOnAdd(); // Use database sequence to generate Id

            // Add unique constraint for RoleId + PermissionId to prevent duplicates
            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for better performance
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => ur.UserId);

            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => rp.RoleId);

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure EventTypesModel
            modelBuilder.Entity<EventTypesModel>()
                .HasIndex(et => et.Name)
                .IsUnique();

            // Configure EventsModel relationship with EventTypesModel
            modelBuilder.Entity<EventsModel>()
                .HasOne(e => e.EventType)
                .WithMany()
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure EventsModel relationship with User (CreatedBy)
            modelBuilder.Entity<EventsModel>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserEventsModel many-to-many relationship
            modelBuilder.Entity<UserEventsModel>()
                .HasKey(ue => ue.Id); // Use Id as primary key, not composite

            modelBuilder.Entity<UserEventsModel>()
                .Property(ue => ue.Id)
                .ValueGeneratedOnAdd(); // Use database sequence to generate Id

            // Add unique constraint for UserId + EventId to prevent duplicates
            modelBuilder.Entity<UserEventsModel>()
                .HasIndex(ue => new { ue.UserId, ue.EventId })
                .IsUnique();

            modelBuilder.Entity<UserEventsModel>()
                .HasOne(ue => ue.User)
                .WithMany()
                .HasForeignKey(ue => ue.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserEventsModel>()
                .HasOne(ue => ue.Event)
                .WithMany()
                .HasForeignKey(ue => ue.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for better performance
            modelBuilder.Entity<UserEventsModel>()
                .HasIndex(ue => ue.UserId);

            modelBuilder.Entity<UserEventsModel>()
                .HasIndex(ue => ue.EventId);

            // Configure EventSettingsModel relationship with User
            modelBuilder.Entity<EventSettingsModel>()
                .HasKey(es => es.Id); // Use Id as primary key

            modelBuilder.Entity<EventSettingsModel>()
                .Property(es => es.Id)
                .ValueGeneratedOnAdd(); // Use database sequence to generate Id

            // Add unique constraint for UserId to ensure one settings record per user
            modelBuilder.Entity<EventSettingsModel>()
                .HasIndex(es => es.UserId)
                .IsUnique();

            modelBuilder.Entity<EventSettingsModel>()
                .HasOne(es => es.User)
                .WithMany()
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
