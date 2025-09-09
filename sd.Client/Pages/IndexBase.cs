using AKSoftware.Localization.MultiLanguages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using sd.Client.Models;
using sd.Client.Services;
using sd.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [Inject]
        public ILogger<IndexBase> Log { get; set; }

        [Inject]
        public WordDtosState WordDtosState { get; set; }

        protected bool CollapsedFriend { get; set; } = true;    // hide by default

        protected Dictionary<string, string> FriendRequestsDictionary = new();

        //protected List<WordDto> AppState.Model { get; set; } = new List<WordDto>();
        protected int NewWords { get; set; }


        protected bool loading;
        protected int currentPage = 1;

        protected void AddNewWord()
        {
            NewWords++;
            WordDtosState.Model.Insert(0, new WordDto { WordId = NewWords.ToString(), WordLang = DefaultLangsService.DefaultWordLang, ToLang = DefaultLangsService.DefaultToLang, ShareWith = ShareWith.Public });
        }

        protected async Task GetNextPage()
        {
            loading = true;
            var wordsCountBefor = WordDtosState.Model.Count;
            WordDtosState.Model.AddRange(await ApiService.GetAsync<List<WordDto>>($"api/Word/GetPageWords/0/10/{currentPage}"));
            currentPage += WordDtosState.Model.Count - wordsCountBefor;
            loading = false;
        }

        protected void NewWordHandler(WordDto word)
        {
            int index = -1; // Initialize with an invalid index

            if (WordDtosState.Model.Any(w => w.WordId == word.WordId))
            {
                index = WordDtosState.Model.FindLastIndex(w => w.WordId == word.WordId);
                WordDtosState.Model.RemoveAt(index); // Remove the old word
            }
            NewWords++;
            WordDtosState.Model.Insert(index, word);
            currentPage++;
        }

        protected void OldWordHandler(WordDto oldWord)
        {
            if (!WordDtosState.Model.Exists(w => w.WordId == oldWord.WordId))
            {
                WordDtosState.Model.Insert(WordDtosState.Model.Count, oldWord);
            }
        }

        protected void DeleteWordHandler(WordDto word)
        {
            int index = WordDtosState.Model.FindIndex(w => w.Equals(word));
            if (index != -1)
            {
                WordDtosState.Model.RemoveAt(index);
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

            if (WordDtosState.Model.Count == 0)
                WordDtosState.Model.Insert(0, new WordDto { WordId = NewWords.ToString(), WordLang = DefaultLangsService.DefaultWordLang, ToLang = DefaultLangsService.DefaultToLang, ShareWith = ShareWith.Public });

            string uiLang = await LocalStorageAccessor.GetValueAsync<string>("UILang");

            if (!string.IsNullOrWhiteSpace(uiLang) && uiLang != "null")
            {
                try
                {
                    LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(uiLang));
                }
                catch
                {
                    Log.LogError($"SetLanguage for uiLang: {uiLang}");
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && CurrentUser.IsAuthenticated)
            {
                FriendRequestsDictionary = await ApiService.GetAsync<Dictionary<string, string>>($"api/Relationship/GetFriendRequests");
            }
        }
    }
}
