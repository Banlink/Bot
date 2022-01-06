using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Banlink.Commands;
using Banlink.Handlers;
using Banlink.Utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Banlink
{
    internal static class Banlink
    {
        public const string ConfigPath = "config.toml";
        public static DiscordClient Client;
        public static DiscordWebhookClient Hook;
        public static string Time { get; private set; }

        private static void Main()
        {
            ServerLinking.GetDriver();
            if (!File.Exists(ConfigPath))
            {
                Console.WriteLine("No config file located! You have one minute to add it...");
                Console.WriteLine(Assembly.GetCallingAssembly().Location);
                Thread.Sleep(60000);
            }

            Time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var config = Configuration.ReadConfig(ConfigPath);
            Hook = new DiscordWebhookClient();
            Hook.AddWebhookAsync(ulong.Parse(config.Webhook.Split("|")[0]),
                config.Webhook.Split("|")[1]);

            MainAsync(config).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(Configuration.Config config)
        {
            // Create the Discord client
            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Information,
                Intents = DiscordIntents.AllUnprivileged
            });

            var commandConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] {config.Prefix},
                EnableDms = false,
                EnableDefaultHelp = false
            };

            // events
            Client.GuildBanAdded += GuildBansHandler.BanHandler;
            Client.GuildBanRemoved += GuildBansHandler.UnbanHandler;

            Client.GuildCreated += ClientOnGuildCreated;
            Client.GuildDeleted += ClientOnGuildDeleted;

            var commands = Client.UseCommandsNext(commandConfig);

            // Register the commands
            commands.RegisterCommands<ServerLinking>();
            commands.RegisterCommands<Misc>();
            commands.RegisterCommands<TestCommands>();
            commands.RegisterCommands<BotOwnerMisc>();

            if (!string.IsNullOrEmpty(config.UptimeKuma))
            {
                Console.WriteLine("Detected kuma URL. Activating uptime logging!");
                var timer = new Timer(30000);
                timer.Elapsed += Uptime.ContactUptimeKuma;
                timer.AutoReset = true;
                timer.Enabled = true;
            }
            else
            {
                Console.WriteLine("No kuma url!");
            }

            // Login and connect
            await Client.ConnectAsync();
            await Task.Delay(2000); // short delay for it connect or it gets mad

            Logger.Log(Logger.LogLevel.Info, "Bot successfully logged in as " +
                                             $"{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}, " +
                                             $"Ping: {Client.Ping}");


            await Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
            {
                IsTTS = false,
                Content = $"[{Time}] Bot is running!"
            });

            await Client.UpdateStatusAsync(new DiscordActivity
            {
                Name = $"{config.Prefix}help",
                ActivityType = ActivityType.Competing
            }, UserStatus.Online);

            await Task.Delay(-1);
        }

        private static async Task ClientOnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
        {
            await Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
            {
                IsTTS = false,
                Content = $"[{Time}] Bot was removed from a server!" +
                          $"\n{e.Guild.Name} - {e.Guild.Id}"
            });
        }

        private static async Task ClientOnGuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            await Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
            {
                IsTTS = false,
                Content = $"[{Time}] Bot was added to new server!" +
                          $"\n{e.Guild.Name} - {e.Guild.Id} - {e.Guild.MemberCount} members!"
            });
        }
    }
}