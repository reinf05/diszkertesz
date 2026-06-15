using diszkerteszAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace diszkerteszAPI
{
    public class diszkerteszDbContext : DbContext
    {
        public diszkerteszDbContext(DbContextOptions<diszkerteszDbContext> options) : base(options)
        {
        }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<Pathogen> Pathogens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UsersShared> UsersShared { get; set; }
        public DbSet<Translate> Translate { get; set; }
        public DbSet<PlantName> PlantNames { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<PlantTips>();

            //// Tell EF Core how to handle the List<string> HungarianNames
            //modelBuilder.Entity<Translate>()
            //    .Property(e => e.HungarianNames)
            //    .HasConversion(
            //        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            //        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
            //             ?? new List<string>()
            //    );

            // Tell EF Core how to handle the PlantTips object
            modelBuilder.Entity<Translate>()
                .Property(e => e.Tips)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<PlantTips>(v, (JsonSerializerOptions)null)
                         ?? new PlantTips()
                );

            // Explicitly configure the Many-to-Many relationship
            modelBuilder.Entity<Detail>()
                .HasMany(d => d.Pathogens)
                .WithMany(p => p.Details)
                .UsingEntity<Dictionary<string, object>>(
                    "Details_Pathogens_Map",

                    // The Foreign Key pointing to the Pathogens table
                    right => right.HasOne<Pathogen>().WithMany().HasForeignKey("PathogenID"),

                    // The Foreign Key pointing exactly to your custom PK in the Details table
                    left => left.HasOne<Detail>().WithMany().HasForeignKey("Plant_ID")
                );
        }
    }
}
