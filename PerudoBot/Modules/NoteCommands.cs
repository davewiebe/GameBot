using Discord.Commands;
using PerudoBot.Data;
using PerudoBot.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("log")]
        public async Task Log(params string[] stringArray)
        {
            await Notes(stringArray);
        }

        [Command("note")]
        public async Task Note(params string[] stringArray)
        {
            await Notes(stringArray);
        }

        [Command("notes")]
        public async Task Notes(params string[] stringArray)
        {
            var game = GetGame(GameState.InProgress);

            if (game == null)
            {
                var now = DateTime.Now.AddMinutes(-5);
                game = _db.Games.AsQueryable()
                    .Where(x => x.ChannelId == Context.Channel.Id)
                    .Where(x => x.State == (int)(object)GameState.Finished)
                    .Where(x => x.DateFinished > now)
                    .OrderByDescending(x => x.Id)

                    .First();
            }
            var text = string.Join(" ", stringArray)
                .StripSpecialCharacters();

            if (text.Length > 256)
            {
                await SendMessage("Note is too long.");
                return;
            }

            RemoveUserCommand();

            _db.Notes.Add(new Note
            {
                Game = game,
                Username = Context.User.Username,
                Text = text
            });

            _db.SaveChanges();

            await SendMessage($"{GetUserNickname(Context.User.Username)} notes: {text}");
        }
    }
}