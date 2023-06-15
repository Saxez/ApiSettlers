using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Reflection.Emit;
using System.Xml;

namespace Project1.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public AppDbContext ()
        {
            
        }

        public DbSet<User> Users { get; set; }

        public DbSet<DifferenceDataHotels> DifferenceDataHotel { get; set; }

        public DbSet<EnteredDataHotel> EnteredDataHotel { get; set; }

        public DbSet<Groups> Groups { get; set; }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<TypesOfDays> TypesOfDays { get; set; }
        

        public DbSet<MassEvent> MassEvents { get; set; }

        public DbSet<RecordDataHotel> RecordDataHotel { get; set; }

        public DbSet<Settler> Settler { get; set; }


        public DbSet<UserXHotel> UserXHotels { get; set; }

        public DbSet<Record> Records { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            modelBuilder.Entity<UserXHotel>().HasKey(u => new { u.UserId, u.HotelId });
            modelBuilder.Entity<TypesOfDays>().HasKey(u => new { u.Name, u.HotelId });
            base.OnModelCreating(modelBuilder);

        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {


            var builder = new ConfigurationBuilder();

            builder.SetBasePath(Directory.GetCurrentDirectory());

            builder.AddJsonFile("appsettings.json");

            var config = builder.Build();

            string connectionString = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

        }
    }
}
