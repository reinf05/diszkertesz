using diszkerteszAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace diszkerteszAPI
{
    public class diszkerteszDbContext : DbContext
    {
        public diszkerteszDbContext(DbContextOptions<diszkerteszDbContext> options) : base(options)
        {
        }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<Detail> Details { get; set; }
    }
}
