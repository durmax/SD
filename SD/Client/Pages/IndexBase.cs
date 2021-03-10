using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using System.Threading.Tasks;
using SD.Shared;
using System.Collections.Generic;
using AKSoftware.Localization.MultiLanguages;
using System.Linq;

namespace SD.Client.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }
        [Inject]
        public RelationshipService RelationshipService { set; get; }
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

        protected bool loading;
        protected int currentPage = 0;
        protected string TLang;

        protected async Task GetNextPage()
        {
            loading = true;
            TLang = DefaultLangsService.DefaultToLang;
            currentPage++;
            var res = await WordService.GetPageWordsFromAllUseres(CurrUsrService.id, 10, currentPage);
            if (res != null)
            {
                currentPage = res.Item1;
                Words.AddRange(res.Item2);
                //Words = Words.Union(res.Item2).Distinct().ToList();
            }
            loading = false;
        }
        protected void NewWordHandler(WordModel newWord)
        {
            Words.Add(newWord);
        }
        protected void DeleteWordHandler(WordModel word)
        {
            Words.Remove(word);
        }
        protected override async Task OnInitializedAsync()
        {
            if (!CurrUsrService.isAuthTested) await CurrUsrService.GetAuth();

            string uiLang = await LocalStorageService.GetItemAsync<string>("UILang");

            if (!string.IsNullOrWhiteSpace(uiLang) && uiLang != "null")
            {
                try
                {
                    languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(uiLang));
                }
                catch { }
            }

            try
            {
                FriendRequestsDictionary = await RelationshipService.GetFriendRequestsById(CurrUsrService.id);
            }
            catch
            {

            }
            
        }
    }
}
