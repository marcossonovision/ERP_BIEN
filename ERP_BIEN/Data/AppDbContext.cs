using ERP_BIEN.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ===== ENTIDADES GENERALES =====
        public DbSet<User> Users { get; set; }

        public DbSet<PreceptorDetails> PreceptorDetails { get; set; }
        public DbSet<PersonalInformation> PersonalInformation { get; set; }
        public DbSet<CompanyInformation> CompanyInformation { get; set; }

        // ===== LICENCIAS / SOFTWARE =====
        public DbSet<License> Licenses { get; set; }
        public DbSet<Software> Software { get; set; }

        // ===== HARDWARE =====
        public DbSet<Hardware> Hardware { get; set; }

        // ===== DISPOSITIVOS (TPH) =====
        public DbSet<Device> Devices { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Ubikey> Ubikeys { get; set; }
        public DbSet<DockStation> DockStations { get; set; }

        // ===== ROLES / PERMISOS =====
        public DbSet<Team> Teams { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<Ascendant> Ascendants { get; set; }
        public DbSet<Child> Childs { get; set; }
        // ===== AUDITORÍA =====
        public DbSet<AuditLog> AuditLogs { get; set; }   // ⭐ AÑADIDO


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            // =========================
            // USER
            // =========================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.LastName)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(u => u.DomainUser)
                      .HasMaxLength(200);

                // 1:N User -> Devices
                entity.HasMany(u => u.Devices)
                      .WithOne(d => d.User)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:N User -> Licenses
                entity.HasMany(u => u.Licenses)
                      .WithOne(l => l.User)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:1 User -> PersonalInformation
                entity.HasOne(u => u.PersonalInfo)
                      .WithOne()
                      .HasForeignKey<User>(u => u.PersonalInfoId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 1:1 User -> CompanyInformation
                entity.HasOne(u => u.CompanyInfo)
                      .WithOne()
                      .HasForeignKey<User>(u => u.CompanyInfoId)
                      .OnDelete(DeleteBehavior.Cascade);

                // N:1 User -> Team (membership)
                entity.HasOne(u => u.Team)
                      .WithMany(t => t.Members)
                      .HasForeignKey(u => u.TeamId)
                      .OnDelete(DeleteBehavior.SetNull);

                // 1:N User -> ManagedTeams (PM / manager)
                entity.HasMany(u => u.ManagedTeams)
                      .WithOne(t => t.ProjectManager)
                      .HasForeignKey(t => t.ProjectManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // =========================
            // TEAM
            // =========================
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                      .IsRequired()
                      .HasMaxLength(150);
            });


            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                entity.HasOne(rp => rp.Role)
                      .WithMany(r => r.RolePermissions)
                      .HasForeignKey(rp => rp.RoleId);

                entity.HasOne(rp => rp.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(rp => rp.PermissionId);
            });



            modelBuilder.Entity<UserRole>(entity =>
            {
                // ✅ Clave primaria compuesta
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                // Relación con User
                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relación con Role
                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });




            base.OnModelCreating(modelBuilder);
        }
    }
}
