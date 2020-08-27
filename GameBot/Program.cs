using System;
using System.IO;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GameBot.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using GameBot.Services;

namespace GameBot
{
    partial class Program
    {
        static void Main(string[] args) => new Program().RunBotASync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private IConfigurationRoot _configuration;

        private string _botType = "";

        public async Task RunBotASync()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_configuration)
                .AddSingleton(new AudioService())
                .AddEntityFrameworkSqlServer()
                .AddDbContext<GameBotDbContext>(options =>
                    options.UseSqlServer(_configuration.GetConnectionString("GameBotDb")))
                .BuildServiceProvider();

            SeedDatabase();

            var token = _configuration.GetSection("DiscordToken").Value;

            _botType = _configuration.GetSection("BotType").Value;

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
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            IResult result = null;

            if (_botType == "deepthought")
            {
                if (message.MentionedUsers.FirstOrDefault()?.Id == context.Client.CurrentUser.Id)
                {
                    result = await _commands.ExecuteAsync(context, "thoughtsxyz", _services);
                }
            }
            else if (_botType == "perudo")
            {
                var argPos = 0;
                if (message.HasStringPrefix("!", ref argPos))
                {
                    result = await _commands.ExecuteAsync(context, argPos, _services);
                }
                else if (message.HasStringPrefix(context.Client.CurrentUser.Mention, ref argPos))
                {
                    result = await _commands.ExecuteAsync(context, argPos+1, _services);
                }
            }

            else if (_botType == "marvin")
            {
                if (message.MentionedUsers.FirstOrDefault()?.Id == context.Client.CurrentUser.Id)
                {
                    result = await _commands.ExecuteAsync(context, "deepxyz", _services);
                }
            }

            else if (_botType == "jif")
            {
                if (message.Content.StartsWith("!jif"))
                {
                    result = await _commands.ExecuteAsync(context, "jifxyz", _services);
                }
            }

            else if (_botType == "gif")
            {
                if (message.Content.StartsWith("!gif"))
                {
                    result = await _commands.ExecuteAsync(context, "gifxyz" + message.Content.Substring(4), _services);
                }
            }

            else if (_botType == "karma")
            {
                var argPos = 0;
                if (message.Content.EndsWith("++") || message.Content.EndsWith("+=1") || message.Content.EndsWith("--") || message.Content.EndsWith("-=1"))
                {
                    result = await _commands.ExecuteAsync(context, "givekarma", _services);
                }
                else if (message.HasStringPrefix("!", ref argPos))
                {
                    result = await _commands.ExecuteAsync(context, argPos, _services);
                }
            }

            if (result != null)
            {
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
