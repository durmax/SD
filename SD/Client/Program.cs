using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using SD.Client.Services;
using SD.Client.Models;
using AKSoftware.Localization.MultiLanguages;
using System.Reflection;
using SD.Shared;
using SD.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            //builder.Services.AddScoped(sp => new HttpClient {}    it is recomenden from Microsoft
            builder.Services.AddSingleton(new HttpClient {
                BaseAddress = new Uri("https://sdapi20200529140234.azurewebsites.net/") }); 
                              //new Uri("https://localhost:44394/") });
           
            builder.Services.AddMsalAuthentication(options =>
            {
                var config = options.ProviderOptions;
                config.Authentication.Authority = "https://login.microsoftonline.com/common";
                config.Authentication.ClientId = "cbae27bd-5b20-43c3-931a-c125881b56a4";
                config.Authentication.ValidateAuthority = true;
                config.Cache.CacheLocation = "localStorage";
                config.Authentication.PostLogoutRedirectUri = "/";
 
                //https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-microsoft-accounts?view=aspnetcore-3.1
            });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSingleton<LangCodeService>();
            builder.Services.AddSingleton<KnownLangsService>();
            builder.Services.AddSingleton<DefaultLangsService>();
            builder.Services.AddSingleton<CurrentUser>();

            builder.Services.AddScoped<CurrentUserService>();

            builder.Services.AddSingleton<UriService>();
            builder.Services.AddSingleton<LinkModel>();
            builder.Services.AddSingleton<LinkParam>();
            builder.Services.AddSingleton<OtherPageService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<WordService>();
            builder.Services.AddSingleton<CommentService>();
            builder.Services.AddSingleton<RelationshipService>();

            

            builder.Services.AddLanguageContainer(Assembly.GetExecutingAssembly());

            await builder.Build().RunAsync();