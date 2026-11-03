
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using PruebaTecnica_MonoLegal.Models;

namespace PruebaTecnica_MonoLegal.Context
{
    public class AppDBContext : DbContext // acceso a los datos
    {
        public AppDBContext(
            DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }
        // Definición de las colecciones en la base de datos
        public DbSet<User> users { get; set; }
        public DbSet<Client> clients { get; set; }
        public DbSet<Invoice> invoices { get; set; }
        public DbSet<Notifications> notifications { get; set; }
        public DbSet<ReminderSettings> reminderSettings { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .ToCollection("users");

            modelBuilder.Entity<Client>()
                .ToCollection("clients");

            modelBuilder.Entity<Invoice>()
                .ToCollection("invoices");

            modelBuilder.Entity<Notifications>()
                .ToCollection("notifications");

            modelBuilder.Entity<ReminderSettings>()
                 .ToCollection("reminderSettings");
        }
    }
}
