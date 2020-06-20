using Discord.Commands;
using GameBot.Data;
using GameBot.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GameBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private GameBotDbContext _db;
        private KarmaService _karmaService;
        private UserService _userService;
        private PhraseService _phraseService;

        public Commands()
        {
            _db = new GameBotDbContext();
            _phraseService = new PhraseService(_db);
        }

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }


        [Command("highscore")]
        public async Task Highscore()
        {
            var monkey = _db.Scores.FirstOrDefault();
            if (monkey == null)
            {
                await ReplyAsync("No scores yet");
                return;
            }
            await ReplyAsync($"Highest score: {monkey.Points}");
        }


        [Command("nothanks")]
        public async Task NoThanks()
        {
            await ReplyAsync("Oh boy");
            Thread.Sleep(500);

            await ReplyAsync("Add users with !add");

        }

        [Command("add")]
        public async Task AddUserToGame()
        {
            await ReplyAsync($"{Context.User.Username} added to game");
        }

        [Command("start")]
        public async Task Start()
        {
            await ReplyAsync($"Starting the game!");


            var deck = new ClassicDeck();
        }
    }
}
