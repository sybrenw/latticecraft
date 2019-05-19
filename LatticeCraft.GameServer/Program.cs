using Lattice.Core;
using LatticeCraft.GameServer.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LatticeCraft.GameServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureLogging((hostContext, config) =>
                {
                    config.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    config.AddConsole();
                    config.AddDebug();
                })            
                .ConfigureServices(ConfigureServices)
                .Build();
            
            AppLogging.LoggerFactory = host.Services.GetService<ILoggerFactory>();

            var server = new MinecraftServer();
            var task1 = server.ListenAsync("127.0.0.1", 25565);

            var task2 = host.RunAsync();

            await Task.WhenAll(task1, task2);
        }
        
        public static void ConfigureServices(IServiceCollection services)
        {

        }
    }
}
