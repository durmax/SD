using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using System.Threading.Tasks;
using SD.Shared;
using System.Collections.Generic;
using System;
using AKSoftware.Localization.MultiLanguages;

namespace SD.Client.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Inject]
        public UserService UserService { set; get; }
        [Inject]
        public ILanguageContainerService languageContainer { set; get; }

        protected WordModel wordModel { get; set; } = new WordModel();

        protected UserModel userModel { get; set; } = new UserModel();
        protected int? FriendRequestsCount { set; get; }

        protected bool Collapsed { get; set; } = true;    // hide by default

        protected Dictionary<string, string> FriendRequestsDictionary;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected string UserId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            string uiLang = await LocalStorageService.GetItemAsync<string>("UILang");
            if (!string.IsNullOrWhiteSpace(uiLang) && uiLang != "null")
            {
                try
                {
                    languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(uiLang));
                }
                catch { }
            }


            wordModel.WordLang = await LocalStorageService.GetItemAsync<string>("FLang");
            wordModel.ToLang = await LocalStorageService.GetItemAsync<string>("TLang");

            if (string.IsNullOrWhiteSpace(wordModel.WordLang) || wordModel.WordLang == "null" || string.IsNullOrWhiteSpace(wordModel.ToLang) || wordModel.ToLang == "null")
            {
                NavigationManager.NavigateTo("Languages");
            }

            var user = (await authenticationStateTask).User;

            if (user.Identity.IsAuthenticated)
            {
                UserId = user.FindFirst(c => c.Type == "oid")?.Value;
                try
                {
                    wordModel.UserId = UserId;
                    wordModel.WordId = Guid.NewGuid().ToString();
                    wordModel.CreatedAt = DateTime.Now;

                    userModel = await UserService.GetUserById(UserId);
                }
                catch
                {
                    userModel.UserId = UserId;
                    userModel.Email = user.FindFirst(c => c.Type == "email")?.Value;
                    userModel.Name = user.Identity.Name;//user.FindFirst(c => c.Type == ClaimTypes.Surname)?.Value
                    await UserService.AddUser(userModel);
                }
                if (userModel.FriendRequests != null)
                {
                    FriendRequestsCount = userModel?.FriendRequests.Count ?? 0;
                    if (FriendRequestsCount > 0)
                    {
                        FriendRequestsDictionary = new Dictionary<string, string>();
                        foreach (var id in userModel.FriendRequests)
                        {
                            UserModel user1 = await UserService.GetUserById(id);
                            FriendRequestsDictionary.Add(user1.UserId, user1.Name);
                        }
                    }
                }
            }
        }
    }
}
