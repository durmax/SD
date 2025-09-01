using Microsoft.AspNetCore.Components;
using sd.Client.Services;
using System.Threading.Tasks;
using sd.Shared;
using System.Collections.Generic;
using AKSoftware.Localization.MultiLanguages;
using System.Linq;

namespace sd.Client.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        LocalStorageAccessor LocalStorageAccessor { get; set; }
        [Inject]
        public ILanguageContainerService LanguageContainer { set; get; }
        [Inject]
        public CurrentUserService CurrentUser { set; get; }
        [Inject]
        protected ApiService ApiService { get; set; }
        [Inject]
        public DefaultLangsService DefaultLangsService { get; set; }

        protected bool CollapsedFriend { get; set; } = true;    // hide by default

        protected Dictionary<string, string> FriendRequestsDictionary = new();

        protected List<WordDto> Words { get; set; } = new List<WordDto>();
        protected int NewWords { get; set; }


        protected bool loading;
        protected int currentPage = 1;

        protected void AddNewWord()
        {
            NewWords++;
            Words.Insert(0, new WordDto { WordId = NewWords.ToString(), WordLang = DefaultLangsService.DefaultWordLang, ToLang = DefaultLangsService.DefaultToLang, ShareWith = ShareWith.Public });
        }

        protected void AddNewWord(WordDto word, int index)
        {
            NewWords++;
            Words.Insert(index, word);
        }

        protected async Task GetNextPage()
        {
            loading = true;
            var wordsCountBefor = Words.Count;
            Words.AddRange(await ApiService.GetAsync<List<WordDto>>($"api/Word/GetPageWords/0/10/{currentPage}"));
            currentPage += Words.Count - wordsCountBefor;
            loading = false;
        }
        protected void NewWordHandler(WordDto word)
        {
            int index = -1; // Initialize with an invalid index

            if (Words.Any(w => w.WordId == word.WordId))
            {
                index = Words.FindLastIndex(w => w.WordId == word.WordId);
                Words.RemoveAt(index); // Remove the old word
            }
            AddNewWord(word, index);
            currentPage++;
        }
        protected void OldWordHandler(WordDto oldWord)
        {
            if (!Words.Exists(w => w.WordId == oldWord.WordId))
            {
                Words.Insert(Words.Count, oldWord);
            }
        }
        protected void DeleteWordHandler(WordDto word)
        {
            int index = Words.FindIndex(w => w.Equals(word));
            if (index != -1)
            {
                Words.RemoveAt(index);
            }
            currentPage--;
        }

        protected override async Task OnInitializedAsync()
        {
            if (!NavigationManager.Uri.Contains("https://www.") && !NavigationManager.Uri.Contains("localhost"))
            {
                NavigationManager.NavigateTo("https://www.lingoclub.net/", true);
            }
            await DefaultLangsService.SetDefLangsAsync();

            Words.Insert(0, new WordDto { WordId = NewWords.ToString(), WordLang = DefaultLangsService.DefaultWordLang, ToLang = DefaultLangsService.DefaultToLang, ShareWith= ShareWith.Public });


            string uiLang = await LocalStorageAccessor.GetValueAsync<string>("UILang");

            if (!string.IsNullOrWhiteSpace(uiLang) && uiLang != "null")
            {
                try
                {
                    LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(uiLang));
                }
                catch { }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && CurrentUser.IsAuthenticated)
            {
                try
                {
                    FriendRequestsDictionary = await ApiService.GetAsync<Dictionary<string, string>>($"api/Relationship/GetFriendRequests");
                }
                catch
                {
                }
            }
        }
    }
}
