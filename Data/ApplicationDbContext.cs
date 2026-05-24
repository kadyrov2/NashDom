using Microsoft.EntityFrameworkCore;
using NashDom.Models;
using Npgsql;

namespace NashDom.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<PersonalAccount> PersonalAccounts { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }
        public DbSet<RegistrationCard> RegistrationCards { get; set; }
        public DbSet<MeterReplacement> MeterReplacements { get; set; }
        public DbSet<Accrual> Accruals { get; set; }
        public DbSet<OrganizationSettings> OrganizationSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            var connectionString = "Host=localhost;Port=5432;Database=NashDomDB;Username=postgres;Password=postgres123";
            
            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Добавляем политику повторных попыток для временных ошибок
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Связь Apartment -> PersonalAccounts
            modelBuilder.Entity<Apartment>()
                .HasMany(a => a.PersonalAccounts)
                .WithOne(p => p.Apartment)
                .HasForeignKey(p => p.ApartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь Apartment -> MeterReadings
            modelBuilder.Entity<Apartment>()
                .HasMany(a => a.MeterReadings)
                .WithOne(m => m.Apartment)
                .HasForeignKey(m => m.ApartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь Apartment -> RegistrationCard (один к одному)
            modelBuilder.Entity<Apartment>()
                .HasOne(a => a.RegistrationCard)
                .WithOne(r => r.Apartment)
                .HasForeignKey<RegistrationCard>(r => r.ApartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь Apartment -> MeterReplacements
            modelBuilder.Entity<Apartment>()
                .HasMany(a => a.MeterReplacements)
                .WithOne(m => m.Apartment)
                .HasForeignKey(m => m.ApartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы
            modelBuilder.Entity<Apartment>()
                .HasIndex(a => a.ApartmentNumber)
                .IsUnique();

            modelBuilder.Entity<PersonalAccount>()
                .HasIndex(p => p.AccountNumber)
                .IsUnique();
        }
    }
}