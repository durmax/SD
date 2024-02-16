using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using System.Threading.Tasks;
using SD.Shared;
using System.Collections.Generic;
using AKSoftware.Localization.MultiLanguages;
using System.Linq;
using System.Net.Http.Json;

namespace SD.Client.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        public ILanguageContainerService LanguageContainer { set; get; }
        [Inject]
        public CurrentUserService CurrentUser { set; get; }
        [Inject]
        public DefaultLangsService DefaultLangsService { get; set; }

        protected bool CollapsedFriend { get; set; } = true;    // hide by default

        protected Dictionary<string, string> FriendRequestsDictionary = new();

        protected List<WordDto> Words { get; set; } = new List<WordDto>();
        protected List<bool> NewWords { get; set; } = new List<bool>();

        [Inject]
        public WordService WordService { set; get; }


        protected bool loading;
        protected int currentPage = 1;

        protected void OnSelectedAsync(int selection)
        {
            switch (selection)
            {
                case 0:
                    NewWords.Insert(0, false);
                    break;
                case 1:
                    NewWords.Insert(0, true);
                    break;
                case 2:
                    if (NewWords.Count > 1) NewWords.Remove(NewWords.Last());
                    break;
                default:
                    break;
            }
        }

        protected async Task GetNextPage()
        {
            loading = true;
            var wordsCountBefor = Words.Count;
            Words.AddRange(await WordService.GetPageWords("0", 10, currentPage));
            currentPage += Words.Count - wordsCountBefor;
            loading = false;
        }
        protected void NewWordHandler(WordDto newWord)
        {
            Words.Insert(0, newWord);
            currentPage++;
        }
        protected void OldWordHandler(WordDto oldWord)
        {
            if (!Words.Exists(w => w.WordId == oldWord.WordId))
            {
                Words.Insert(0, oldWord);
            }
        }
        protected void DeleteWordHandler(WordDto word)
        {
            Words.Remove(word);
            currentPage--;
        }
        protected override async Task OnInitializedAsync()
        {
            await DefaultLangsService.SetDefLangsAsync();

            NewWords.Insert(0, false);

           // if (!CurrentUser.isAuthTested) await CurrUsrService.GetAuth();

            string uiLang = await LocalStorageService.GetItemAsync<string>("UILang");

            if (!string.IsNullOrWhiteSpace(uiLang) && uiLang != "null")
            {
                try
                {
                    LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(uiLang));
                }
                catch { }
            }

            //if (CurrentUser.IsAuthenticated)
            //{
            //    try
            //    {
            //        FriendRequestsDictionary = await CurrentUser.HttpClient.GetFromJsonAsync<Dictionary<string, string>>($"api/Relationship/GetFriendRequests");
            //    }
            //    catch
            //    {
            //    }
            //}
        }
    }
}
