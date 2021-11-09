using System;
using System.Threading.Tasks;
using Banlink.Commands;
using Banlink.Utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Banlink
{
    internal static class Banlink
    {
        public static string Time { get; private set; }

        private static async Task Main()
        {
            Time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var config = Configuration.ReadConfig("config.toml");
            MainAsync(config).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(Configuration.Config config)
        {
            // Create the Discord client
            var client = new DiscordClient(new DiscordConfiguration
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Information,
                Intents = DiscordIntents.All
            });

            var commandConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] {config.Prefix},
                EnableDms = false
            };

            var commands = client.UseCommandsNext(commandConfig);
            
            // Register the commands
            commands.RegisterCommands<Test>();

            // Login and connect
            await client.ConnectAsync();
            await Task.Delay(3000); // short delay for it connect or it gets mad

            Logger.Log(Logger.LogLevel.Info, "Bot successfully logged in as " +
                                             $"{client.CurrentUser.Username}#{client.CurrentUser.Discriminator}, " +
                                             $"Ping: {client.Ping}");

            await client.UpdateStatusAsync(new DiscordActivity
            {
                Name = $"{config.Prefix}help",
                ActivityType = ActivityType.Custom
            }, UserStatus.Online);

            await Task.Delay(-1);
        }
    }
}