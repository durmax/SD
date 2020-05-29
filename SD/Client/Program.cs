using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using SD.Client.Services;
using SD.Client.Models;

namespace SD.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://localhost:44322/") });
            builder.Services.AddMsalAuthentication(options =>
            {
                var authentication = options.ProviderOptions.Authentication;
                authentication.Authority = "https://login.microsoftonline.com/common";
                authentication.ClientId = "c7b4ae4a-ce9a-42aa-ad16-1c6f94c9a003";
            });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSingleton<LangCodeService>();
            builder.Services.AddTransient<UriService>();
            builder.Services.AddTransient<LinkModel>();
            builder.Services.AddTransient<LinkParam>();

            builder.Services.AddTransient<OtherPageService>();
            await builder.Build().RunAsync();
        }
    }
}
