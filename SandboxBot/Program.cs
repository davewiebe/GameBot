using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GameBot.Services;
using Serilog;

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
            //Log.Logger = new LoggerConfiguration()
            //.MinimumLevel.Debug()
            //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console()
            //    //.WriteTo.File("serilog.txt", rollingInterval: RollingInterval.Day)
            //    .CreateLogger();

            //try
            //{
            //    Log.Information("Starting host");
            //    BuildHost().Run();
            //    //return 0;
            //}
            //catch (Exception ex)
            //{
            //    Log.Fatal(ex, "Host terminated unexpectedly");
            //    //return 1;
            //}
            //finally
            //{
            //    Log.CloseAndFlush();
            //}


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
                .AddLogging(builder => builder.AddSerilog(dispose: true))
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
            //Log.Error(arg.Exception, arg.Exception?.Message);
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

            var argPos = 0;
            if (message.HasStringPrefix("!", ref argPos))
            {
                result = await _commands.ExecuteAsync(context, argPos, _services);
            }
            if (result != null)
            {
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
