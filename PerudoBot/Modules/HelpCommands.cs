using Discord.Commands;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync("Commands:" +
                "\n!new");
        }

        [Command("version")]
        public async Task version(params string[] stringArray)
        {
            {
                await ReplyAsync("`PerudoBot version 1.5.1`\n" +
                    "\n" +
                    "**1.5.1**\n" +
                    "*01-Sept/2020*\n" +
                    "- Added exactanytime, liaranytime\n" +
                    "- Fixed some bugs\n" +
                    "\n" + 
                    "**3.Blue.4**\n" +
                    "*26-Aug-20*\n" +
                    "- added variable numbas o'dice\n" +
                    "- some other stuff, can't remember\n" +
                    "\n" +
                    "**2020.1-1**\n" +
                    "*2020-08-24*\n" +
                    "- Emoji's. Emoji's everywhere\n" +
                    "\n" +
                    "**1.0.0.0.1**\n" +
                    "*2020/08/?*\n" +
                    "- Hotfix some dice issues or something\n" +
                    "\n" +
                    "**2.0**\n" +
                    "*20/0?/?*\n" +
                    "- Fixed wording\n" +
                    "\n" +
                    "**1.0**\n" +
                    "*Date unknown*\n" +
                    "- First release! I'll remember this day forever!\n");

            }
        }
    }  
}
