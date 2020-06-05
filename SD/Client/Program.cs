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

            builder.Services.AddSingleton(new HttpClient {
                BaseAddress = new Uri("https://sdapi20200529140234.azurewebsites.net/") }); 
                           // new Uri("https://localhost:44394/") });
           
            builder.Services.AddMsalAuthentication(options =>
            {
                var authentication = options.ProviderOptions.Authentication;
                authentication.Authority = "https://login.microsoftonline.com/common";
                authentication.ClientId = "cbae27bd-5b20-43c3-931a-c125881b56a4";
                //https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-microsoft-accounts?view=aspnetcore-3.1
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
