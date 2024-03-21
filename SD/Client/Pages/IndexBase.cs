using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using System.Threading.Tasks;
using SD.Shared;
using System.Collections.Generic;
using AKSoftware.Localization.MultiLanguages;
using System.Net.Http.Json;

namespace SD.Client.Pages
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
        public DefaultLangsService DefaultLangsService { get; set; }

        protected bool CollapsedFriend { get; set; } = true;    // hide by default

        protected Dictionary<string, string> FriendRequestsDictionary = new();

        protected List<WordDto> Words { get; set; } = new List<WordDto>();
        protected int NewWords { get; set; }

        [Inject]
        public WordService WordService { set; get; }


        protected bool loading;
        protected int currentPage = 1;

        protected void AddNewWord()
        {
            NewWords++;
            Words.Insert(0, new WordDto { WordId = NewWords.ToString(), WordLang = DefaultLangsService.DefaultWordLang, ToLang = DefaultLangsService.DefaultToLang });
        }

        protected async Task GetNextPage()
        {
            loading = true;
            var wordsCountBefor = Words.Count;
            Words.AddRange(await WordService.GetPageWords("0", 10, currentPage));
            currentPage += Words.Count - wordsCountBefor;
            loading = false;
        }
        protected void NewWordHandler(WordDto word)
        {
            var ws = Words.FindAll(w => w.WordId == word.WordId);
            if (ws.Count > 1) // by update, it will be 2
            {
                int maxIndex = -1; // Initialize with an invalid index

                foreach (var item in ws)
                {
                    int index = Words.FindLastIndex(w => w.WordId == item.WordId); // Find the last index of matching item
                    if (index > maxIndex)
                    {
                        maxIndex = index; // Update maxIndex if a higher index is found
                    }
                }

                Words.RemoveAt(maxIndex); // Remove the old word by update
            }
            AddNewWord();
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
            if (!NavigationManager.Uri.Contains("localhost"))
            {
                if (!NavigationManager.Uri.Contains("https://www.lingoclub.net"))
                {
                    NavigationManager.NavigateTo("https://www.lingoclub.net/");
                }
            }
            await DefaultLangsService.SetDefLangsAsync();

            Words.Insert(0, new WordDto { WordId = NewWords.ToString(), WordLang = DefaultLangsService.DefaultWordLang, ToLang = DefaultLangsService.DefaultToLang });


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
                    FriendRequestsDictionary = await CurrentUser.HttpClient.GetFromJsonAsync<Dictionary<string, string>>($"api/Relationship/GetFriendRequests");
                }
                catch
                {
                }
            }
        }
    }
}
