﻿using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Banlink.Commands
{
    public class BotOwnerMisc : BaseCommandModule
    {
        [Command("guildcount")]
        [Hidden]
        [RequireOwner]
        public async Task guildcount(CommandContext ctx)
        {
            var guilds = ctx.Client.Guilds.Count;
            await ctx.RespondAsync($"The bot is in {guilds} servers!");
        }
    }
}