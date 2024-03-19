using System;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SD.Client.Services;
using SD.Client.Models;
using AKSoftware.Localization.MultiLanguages;
using System.Reflection;
using SD.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("app");

builder.Services.AddSingleton<LoggingService>();
builder.Services.AddScoped<LocalStorageAccessor>();

// Add configured HttpClient with AuthorizationMessageHandler
builder.Services.AddHttpClient("forAuthenticatedUser", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]); // "ApiUrl:Prod" or "ApiUrl:Dev"
}).AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
                                     .ConfigureHandler(
                                                        authorizedUrls: new[] { builder.Configuration["ApiUrl"] },
                                                        scopes: new[] { builder.Configuration["AzureAd:Scope"] }
                                                        ));

// Add configured HttpClient without AuthorizationMessageHandler
builder.Services.AddHttpClient("forNotAuthenticatedUser", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]); // "ApiUrl:Prod" or "ApiUrl:Dev"
});


builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration["AzureAd:Scope"]);
    options.ProviderOptions.LoginMode = "redirect";
    options.ProviderOptions.Cache.CacheLocation = "localStorage"; // remove this option to use Session storage.
}) ;

builder.Services.AddScoped<LangCodeService>();
builder.Services.AddScoped<KnownLangsService>();
builder.Services.AddScoped<DefaultLangsService>();

builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddScoped<UriService>();
builder.Services.AddScoped<LinkModel>();
builder.Services.AddScoped<LinkParam>();
builder.Services.AddScoped<OtherPageService>();
builder.Services.AddScoped<WordService>();

builder.Services.AddLanguageContainer(Assembly.GetExecutingAssembly());

await builder.Build().RunAsync();