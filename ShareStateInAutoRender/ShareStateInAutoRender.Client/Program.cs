using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShareStateInAutoRender.Client.Services;

namespace ShareStateInAutoRender.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddSingleton<ICounterStateResolver, ClientCounterStateResolver>();

            await builder.Build().RunAsync();
        }
    }
}
