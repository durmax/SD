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
        [Inject]
        public CurrentUser CurrentUser { set; get; }
        [Inject]
        public DefaultLangsService DefaultLangsService { get; set; }

        protected bool CollapsedFriend { get; set; } = true;    // hide by default

        protected Dictionary<string, string> FriendRequestsDictionary = new Dictionary<string, string>();

        protected List<WordDto> Words { get; set; } = new List<WordDto>();
        protected List<bool> NewWords { get; set; } = new List<bool>();

        [Inject]
        public WordService WordService { set; get; }


        protected bool loading;
        protected int currentPage = 0;
        protected string TLang;

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
            TLang = DefaultLangsService.DefaultToLang;
            currentPage++;
            var res = await WordService.GetPageWordsFromAllUseres(CurrentUser.id, 10, currentPage);
            if (res != null)
            {
                currentPage = res.Item1;
                Words.AddRange(res.Item2);
                //Words = Words.Union(res.Item2).Distinct().ToList();
            }
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
            NewWords.Insert(0, false);

            if (!CurrentUser.isAuthTested) await CurrUsrService.GetAuth();
       
            string uiLang = await LocalStorageService.GetItemAsync<string>("UILang");

            if (!string.IsNullOrWhiteSpace(uiLang) && uiLang != "null")
            {
                try
                {
                    languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(uiLang));
                }
                catch { }
            }

            if(CurrentUser.isAuthenticated)
            {
                try
                {
                    FriendRequestsDictionary = await RelationshipService.GetFriendRequestsById(CurrentUser.id);
                }
                catch
                {
                }
            }
        }
    }
}
