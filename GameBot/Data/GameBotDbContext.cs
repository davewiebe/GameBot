using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public DbSet<Score> Scores { get; set; }
        public DbSet<Karma> Karma { get; set; }
        public DbSet<Phrase> Phrase { get; set; }
        public DbSet<KeyPhrase> KeyPhrase { get; set; }
        public DbSet<DeepThought> DeepThoughts { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<BotKey> BotKeys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .Build();

                var connectionString = configuration.GetConnectionString("GameBotDb");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
