using NUnit.Framework;
using PerudoBot.Data;
using PerudoBot.Modules;
using PerudoBot.Services;
using PerudoBotTests;
using Shouldly;
using System.Threading.Tasks;

namespace PerudoBotTests.PerudoGameServiceTests
{
    [TestFixture]
    public class GetGameShould
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

        [TestCase(GameState.Setup)]
        [TestCase(GameState.InProgress)]
        [Test]
        public async Task ReturnGameWithState(GameState state)
        {
            ulong channelId = 3;
            var game = new Game { ChannelId = channelId, State = (int)state };
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            var result = await _service.GetGameAsync(channelId, state);

            result.ShouldNotBeNull();
        }

        [Test]
        public async Task NotTerminateFinishedGame()
        {
            var game = new Game { State = (int)GameState.Finished };
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            await _service.TerminateGameAsync(game.Id);

            var newGameState = ((GameState)game.State);
            newGameState.ShouldBe(GameState.Finished);
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