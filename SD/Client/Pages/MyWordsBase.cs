using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class MyWordsBase : ComponentBase
    {
        [Inject]
        public WordService WordService { set; get; }

        [Inject]
        DefaultLangsService DefaultLangsService { get; set; }
        [Inject]
        public CurrentUser CurrentUser { get; set; }

        [Parameter]
        public string UserId { get; set; }

        [Parameter]
        public string UserName { get; set; }
        protected bool Collapsed { set; get; } = true;    // hide by default
        protected bool loading = true;
        protected int currentPage = 0;
        protected string TLang;

        protected List<WordDto> Words { get; set; } = new List<WordDto>();

        protected void NewWordHandler(WordDto wordDto)
        {
            Words.Add(wordDto);
            currentPage++;
        }
        protected void DeleteWordHandler(WordDto wordDto)
        {
            Words.Remove(wordDto);
            currentPage--;
        }

        protected async Task InitAsync()
        {
            TLang = DefaultLangsService.DefaultToLang;
            await GetNextPage();
        }

        protected async Task GetNextPage()
        {
            loading = true;
            var wordsCountBefor = Words.Count;
            Words.AddRange(await WordService.GetPageWordsFromAllUseres(CurrentUser.id, 10, currentPage));
            currentPage += Words.Count - wordsCountBefor;
            loading = false;
        }

        protected override async Task OnParametersSetAsync()
        {
            Words = null;
            currentPage = 0;
            await InitAsync();
        }
    }
}
