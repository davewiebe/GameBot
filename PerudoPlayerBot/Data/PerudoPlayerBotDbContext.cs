using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;

namespace PerudoPlayerBot.Data
{
    public class PerudoPlayerBotDbContext : DbContext
    {
        public PerudoPlayerBotDbContext() : base()
        {
        }

        public PerudoPlayerBotDbContext(DbContextOptions<PerudoPlayerBotDbContext> options) : base(options)
        { }

        public DbSet<Game> Games { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerRound> PlayerRound { get; set; }
        public DbSet<Bid> Bids { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .Build();

                var connectionString = configuration.GetConnectionString("PerudoPlayerBotDb");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
