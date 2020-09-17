using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;

namespace GameBot.Data
{
    public class GameBotDbContext : DbContext
    {
        public GameBotDbContext() : base()
        {
        }

        public GameBotDbContext(DbContextOptions<GameBotDbContext> options) : base(options)
        { }

        public DbSet<DeepThought> DeepThoughts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .Build();

                var connectionString = configuration.GetConnectionString("GameBotDb");
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }
}
