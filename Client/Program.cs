using AKSoftware.Localization.MultiLanguages;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using sd.Client;
using sd.Client.LoggerProvider;
using sd.Client.Models;
using sd.Client.Services;
using System;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("app");

builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddSingleton(new InMemoryLogStore(capacity: 1000));
builder.Logging.AddProvider(new InMemoryLoggerProvider(
    builder.Services.BuildServiceProvider().GetRequiredService<InMemoryLogStore>()));

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

builder.Services.AddFluentUIComponents();

builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<ApiService>();

builder.Services.AddScoped<LangCodeService>();
builder.Services.AddScoped<KnownLangsService>();
builder.Services.AddScoped<DefaultLangsService>();

builder.Services.AddScoped<UriService>();
builder.Services.AddScoped<LinkModel>();
builder.Services.AddScoped<LinkParam>();
builder.Services.AddScoped<OtherPageService>();

builder.Services.AddSingleton<WordDtosState>();

builder.Services.AddLanguageContainer(Assembly.GetExecutingAssembly());

await builder.Build().RunAsync();