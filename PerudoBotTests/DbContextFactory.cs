using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace PerudoBotTests
{
    public class GameBotDbContextFactory : IDisposable
    {
        private DbConnection _connection;

        private DbContextOptions<GameBotDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<GameBotDbContext>()
                .UseSqlite(_connection).Options;
        }

        public GameBotDbContext CreateContext()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                var options = CreateOptions();
                using (var context = new GameBotDbContext(options))
                {
                    context.Database.EnsureCreated();
                }
            }

            return new GameBotDbContext(CreateOptions());
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}