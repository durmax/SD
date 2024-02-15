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

        [Parameter]
        public string UserId { get; set; }

        protected bool Collapsed { set; get; } = true;    // hide by default
        protected bool loading = true;
        protected int currentPage = 1;
        protected List<WordDto> Words { get; set; }

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

        protected async Task GetNextPage()
        {
            loading = true;
            var wordsCountBefor = Words.Count;
            Words.AddRange(await WordService.GetPageWords(UserId, 10, currentPage));
            currentPage += Words.Count - wordsCountBefor;
            loading = false;
        }

        protected override async Task OnInitializedAsync()
        {
            Words = new List<WordDto>();
            await GetNextPage();
        }
    }
}
