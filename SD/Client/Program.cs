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

// Add configured HttpClient with name: SD.Client.ServerAPI. It configured it has access tokens.
builder.Services.AddHttpClient("SD.Client.ServerAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]); // "ApiUrl:Prod" or "ApiUrl:Dev"
}).AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                                     .ConfigureHandler(
                                                        authorizedUrls: new[] { builder.Configuration["ApiUrl"] },
                                                        scopes: new[] { builder.Configuration["AzureAd:Scope"] }
                                                        ));

// Create HttpClient with name: SD.Client.ServerAPI. (see its Configuration)
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SD.Client.ServerAPI"));

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration["AzureAd:Scope"]);
    options.ProviderOptions.LoginMode = "redirect";
}) ;

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