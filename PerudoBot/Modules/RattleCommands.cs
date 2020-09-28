using Discord;
using Discord.Commands;
using PerudoBot.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("rattles")]
        public async Task Rattles()
        {
            await SendMessage("Set your rattles with `!deathrattle`, `!winrattle`, and `!tauntrattle`.\nI've PM'd you your rattles.");

            var user = Context.Message.Author;

            var rattles = _db.Rattles.SingleOrDefault(x => x.Username == user.Username);
            if (rattles != null)
            {
                var message = $"deathrattle: {rattles.Deathrattle}\n" +
                    $"winrattle: {rattles.Winrattle}\n" +
                    $"tauntrattle: {rattles.Tauntrattle}";

                var requestOptions = new RequestOptions()
                { RetryMode = RetryMode.RetryRatelimit };
                await user.SendMessageAsync(message, options: requestOptions);
            }
        }

        [Command("deathrattle")]
        public async Task Deathrattle(params string[] stringArray)
        {
            var username = Context.Message.Author.Username;
            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch { }

            // get current deathrattle
            var currentDr = _db.Rattles.SingleOrDefault(x => x.Username == username);

            if (currentDr == null)
            {
                _db.Rattles.Add(new Rattle
                {
                    Username = username,
                    Deathrattle = string.Join(" ", stringArray)
                });
                _db.SaveChanges();
            }
            else
            {
                currentDr.Deathrattle = string.Join(" ", stringArray);
                _db.SaveChanges();
            }
            await SendMessage($"{username}'s deathrattle updated.");
        }

        [Command("winrattle")]
        public async Task Winrattle(params string[] stringArray)
        {
            var username = Context.Message.Author.Username;
            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch { }

            var currentDr = _db.Rattles.SingleOrDefault(x => x.Username == username);
            if (currentDr == null)
            {
                _db.Rattles.Add(new Rattle
                {
                    Username = username,
                    Winrattle = string.Join(" ", stringArray)
                });
                _db.SaveChanges();
            }
            else
            {
                currentDr.Winrattle = string.Join(" ", stringArray);
                _db.SaveChanges();
            }
            await SendMessage($"{(username)}'s winrattle updated.");
        }

        [Command("tauntrattle")]
        public async Task Tauntrattle(params string[] stringArray)
        {
            var username = Context.Message.Author.Username;
            try
            {
                _ = Context.Message.DeleteAsync();
            }
            catch { }

            var currentDr = _db.Rattles.SingleOrDefault(x => x.Username == username);
            if (currentDr == null)
            {
                _db.Rattles.Add(new Rattle
                {
                    Username = username,
                    Tauntrattle = string.Join(" ", stringArray)
                });
                _db.SaveChanges();
            }
            else
            {
                currentDr.Tauntrattle = string.Join(" ", stringArray);
                _db.SaveChanges();
            }
            await SendMessage($"{(username)}'s tauntrattle updated.");
        }
    }
}