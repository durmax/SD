using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
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
        [Inject]
        public CurrentUserService CurrUsrService { set; get; }

        protected bool Collapsed { get; set; } = true;    // hide by default

        protected Dictionary<string, string> FriendRequestsDictionary = new Dictionary<string, string>();

        protected List<WordModel> Words { get; set; } = new List<WordModel>();
        [Inject]
        public WordService WordService { set; get; }
        [Inject]
        DefaultLangsService DefaultLangsService { get; set; }

        protected string currUserId;
        protected bool loading;
        protected int currentPage = 0;
        protected string TLang;

        protected async Task GetNextPage()
        {
            loading = true;
            TLang = DefaultLangsService.DefaultToLang;

            if (string.IsNullOrWhiteSpace(currUserId)) currUserId = await CurrUsrService.GetCurrUsrId();

            currentPage++;
            var word = await WordService.GetWords(currUserId, TLang, 10, currentPage);
            if (word != null)
            {
                currentPage = word.Item1;
                Words.AddRange(word.Item2);
            }
            loading = false;
        }

        private async Task AddUserAsync(string currUserId, string email, string name)
        {
            UserModel userModel = new UserModel();
            userModel.UserId = currUserId;
            userModel.Email = email;
            userModel.Name = name;
            await UserService.AddUser(userModel);
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

            if (string.IsNullOrWhiteSpace(currUserId)) currUserId = await CurrUsrService.GetCurrUsrId();

            if (currUserId != "0")
            {
                await AddUserAsync(currUserId, 
                                   await CurrUsrService.GetCurrUsrEmail(), 
                                   await CurrUsrService.GetCurrUsrName());
            }

            try
            {
                FriendRequestsDictionary = await UserService.GetFriendRequestsById(currUserId);
            }
            catch
            {

            }
        }

    }
}
