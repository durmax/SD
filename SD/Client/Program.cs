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
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("app");

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
}) ;


var host = builder.Build();

var authenticationStateProvider = host.Services.GetRequiredService<AuthenticationStateProvider>();
var authenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
bool isAuthenticated = authenticationState.User.Identity.IsAuthenticated;

string httpClientName = isAuthenticated ? "forAuthenticatedUser" : "forNotAuthenticatedUser";

// Create HttpClient with name: httpClientName
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName));

await builder.Build().RunAsync();