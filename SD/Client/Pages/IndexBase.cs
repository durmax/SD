using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using System.Threading.Tasks;
using SD.Shared;
using System.Collections.Generic;
using AKSoftware.Localization.MultiLanguages;

namespace SD.Client.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        public UserService UserService { set; get; }
        [Inject]
        public ILanguageContainerService languageContainer { set; get; }

        protected bool Collapsed { get; set; } = true;    // hide by default

        protected Dictionary<string, string> FriendRequestsDictionary = new Dictionary<string, string>();

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected string UserId { get; set; }
        protected List<WordModel> Words { get; set; }
        [Inject]
        public WordService WordService { set; get; }
        [Inject]
        DefaultLangsService DefaultLangsService { get; set; }

        protected bool loading = true;
        protected int currentPage = 0;
        protected string TLang = "";

        protected async Task GetNextPage()
        {
            loading = true;
            currentPage++;

            if (Words == null)
            {
                Words = new List<WordModel>();
            }
            var word = await WordService.GetWords(UserId, TLang, 10, currentPage);
            currentPage = word.Item1;
            Words.AddRange(word.Item2);
            loading = false;
        }

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

            var user = (await authenticationStateTask).User;

            if (user.Identity.IsAuthenticated)
            {
                UserId = user.FindFirst(c => c.Type == "oid")?.Value;
                try
                {
                    FriendRequestsDictionary = await UserService.GetFriendRequestsById(UserId);
                }
                catch
                {
                    UserModel userModel = new UserModel();
                    userModel.UserId = UserId;
                    userModel.Email = user.FindFirst(c => c.Type == "email")?.Value;
                    userModel.Name = user.Identity.Name;//user.FindFirst(c => c.Type == ClaimTypes.Surname)?.Value
                    await UserService.AddUser(userModel);
                }
            }
            TLang = DefaultLangsService.DefaultToLang;
            await GetNextPage();
        }
    }
}
