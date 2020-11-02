using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PerudoBot.Data
{
    public class GameBotDbContext : DbContext
    {
        public GameBotDbContext() : base()
        {
        }

        public GameBotDbContext(DbContextOptions<GameBotDbContext> options) : base(options)
        { }

        public DbSet<Game> Games { get; set; }
        public DbSet<Rattle> Rattles { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<BotKey> BotKeys { get; set; }
        public DbSet<Action> Actions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<LiarCall> LiarCalls { get; set; }
        public DbSet<ExactCall> ExactCalls { get; set; }

        public DbSet<Round> Rounds { get; set; }

        public DbSet<StandardRound> StandardRounds { get; }
        public DbSet<PalificoRound> PalificoRounds { get; }
        public DbSet<FaceoffRound> FaceoffRounds { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Action>()
                .ToTable("Actions")
                .HasDiscriminator<string>("ActionType");

            modelBuilder.Entity<Round>()
                .ToTable("Rounds")
                .HasDiscriminator<string>("RoundType");
        }

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
                // optionsBuilder.UseSnakeCaseNamingConvention();
            }
        }
    }
}