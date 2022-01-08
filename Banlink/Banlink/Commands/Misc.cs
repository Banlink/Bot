using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Banlink.Commands
{
    public class Misc : BaseCommandModule
    {
        [Command("help")]
        public async Task help(CommandContext ctx)
        {
            await ctx.RespondAsync("Welcome to Banlink!" +
                                   "\n" +
                                   "\n Visit https://github.com/Banlink/Bot/wiki/Documentation" +
                                   "\n" +
                                   "To use the bot, first go to a server you want to subscribe to bans and unbans for. " +
                                   "In that server, someone with `Manage Server` permission should run `b>generate`" +
                                   "\nNext, go to any server you want to link and run `b>link <Link Code>`" +
                                   "\nTo unlink a server from your server, type `b>unlink <Server ID>`" +
                                   "\nThat's it!" +
                                   "\nTop tip! Add `NO_BANLINK` in the reason of any ban to make banlink ignore it.");
        }
    }
}