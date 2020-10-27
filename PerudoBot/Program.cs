using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerudoBot.Data;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PerudoBot
{
    partial class Program
    {
        private static void Main(string[] args) =>
            new Program().RunBotASync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private IConfigurationRoot _configuration;

        public async Task RunBotASync()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            var config = new DiscordSocketConfig { MessageCacheSize = 100, AlwaysDownloadUsers = true };
            _client = new DiscordSocketClient(config);
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_configuration)
                .AddLogging(builder => builder.AddSerilog(dispose: true))
                .AddEntityFrameworkSqlServer()
                .AddDbContext<GameBotDbContext>(options =>
                    options.UseNpgsql(_configuration.GetConnectionString("GameBotDb")))
                .BuildServiceProvider();

            var token = _configuration.GetSection("DiscordToken").Value;

            _client.Log += _client_Log;

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _client.ReactionAdded += HandleReactionAddedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> before, ISocketMessageChannel after, SocketReaction reaction)
        {
            var message = await before.GetOrDownloadAsync();

            if (IsReactionMine(reaction) || !IsMessageMine(message)) return;

            var context = new CommandContext(_client, message);
            Console.WriteLine($"Handling reaction of {reaction.Emote.Name}");
            var result = await _commands.ExecuteAsync(context, reaction.Emote.Name, _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null) return;
            var context = new SocketCommandContext(_client, message);

            IResult result = null;

            var prefix = _configuration.GetSection("BotCommandPrefix").Value;

            var argPos = 0;
            if (message.HasStringPrefix(prefix, ref argPos))
            {
                result = await _commands.ExecuteAsync(context, argPos, _services);
            }
            else if (message.HasStringPrefix(context.Client.CurrentUser.Mention, ref argPos))
            {
                result = await _commands.ExecuteAsync(context, argPos + 1, _services);
            }

            if (result != null)
            {
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        private bool IsMessageMine(IUserMessage message)
        {
            return _client.CurrentUser.Id == message.Author.Id;
        }

        private bool IsReactionMine(SocketReaction reaction)
        {
            return _client.CurrentUser.Id == reaction.UserId;
        }
    }
}