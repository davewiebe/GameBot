using NUnit.Framework;
using PerudoBot.Data;
using PerudoBot.Modules;
using PerudoBot.Services;
using PerudoBotTests;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace PerudoBotTests.PerudoGameServiceTests
{
    [TestFixture]
    public class GetCurrentPlayerShould
    {
        private GameBotDbContextFactory _factory;
        private GameBotDbContext _context;
        private PerudoGameService _service;

        [SetUp]
        public void Setup()
        {
            _factory = new GameBotDbContextFactory();
            _context = _factory.CreateContext();
            _service = new PerudoGameService(_context);
        }

        [Test]
        public async Task ReturnPlayerWhoseTurnItIs()
        {
            var player = new Player() { Username = "Jim" };

            _context.Add(player);
            await _context.SaveChangesAsync();

            var game = new Game { PlayerTurnId = player.Id };
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            var result = _service.GetCurrentPlayer(game);

            result.Username.ShouldBe("Jim");
        }

        [Test]
        public async Task ThrowExceptionIfNoneSet()
        {
            var game = new Game { PlayerTurnId = null };
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            Should.Throw<InvalidOperationException>(() => _service.GetCurrentPlayer(game));
        }

        [TearDown]
        public async Task TearDownAsync()
        {
            _service.Dispose();
            await _context.DisposeAsync();
            _factory.Dispose();
        }
    }
}