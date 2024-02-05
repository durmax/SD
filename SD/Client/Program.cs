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
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("app");

builder.Services.AddHttpClient("SD.Client.ServerAPI", client =>
        client.BaseAddress = new Uri("https://sdapi20200529140234.azurewebsites.net/"))
        //client.BaseAddress = new Uri("https://localhost:44394/"))
    
      .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SD.Client.ServerAPI"));


//builder.Services.AddMsalAuthentication(options =>
//            {
//                var config = options.ProviderOptions;
//                config.Authentication.Authority = "https://login.microsoftonline.com/common";
//                config.Authentication.ClientId = "7fdcd008-ffe6-4f0a-8aba-0a6ec41c8e95";
//                config.Authentication.ValidateAuthority = true;
//                config.Cache.CacheLocation = "localStorage";
//                config.Authentication.PostLogoutRedirectUri = "/";

//                //https://docs.microsoft.com/en-us/aspnet/core/security/blazor/webassembly/standalone-with-microsoft-accounts?view=aspnetcore-3.1
//            });


builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://2908957d-5105-40bc-a8fe-05903a20571e/SD.ReadWrite");
    options.ProviderOptions.LoginMode = "redirect";
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<LangCodeService>();
builder.Services.AddScoped<KnownLangsService>();
builder.Services.AddScoped<DefaultLangsService>();
builder.Services.AddScoped<CurrentUser>();

builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddScoped<UriService>();
builder.Services.AddScoped<LinkModel>();
builder.Services.AddScoped<LinkParam>();
builder.Services.AddScoped<OtherPageService>();
builder.Services.AddScoped<WordService>();

builder.Services.AddLanguageContainer(Assembly.GetExecutingAssembly());

await builder.Build().RunAsync();