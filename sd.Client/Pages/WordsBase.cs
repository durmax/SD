using Microsoft.AspNetCore.Components;
using sd.Client.Services;
using sd.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Client.Pages
{
    public class WordsBase : ComponentBase
    {
        [Inject] protected ApiService ApiService { get; set; }
        [Parameter] public string UserId { get; set; }
        public string PageHeader { get; set; }
        protected List<WordDto> Words { get; set; }
        //protected bool Collapsed { set; get; } = true;    // hide by default
        protected bool loading = true;
        protected int currentPage = 1;

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
            Words.AddRange(await ApiService.GetAsync<List<WordDto>>($"api/Word/GetPageWords/0/10/{currentPage}"));

            currentPage += Words.Count - wordsCountBefor;
            loading = false;
        }

        protected override async Task OnInitializedAsync()
        {
            Words = new List<WordDto>();
            await GetNextPage();
            if (Words.Count > 0)
            {
                PageHeader = string.IsNullOrEmpty(UserId) ? "My words" : Words.FirstOrDefault(x => !string.IsNullOrEmpty(x?.UserName))?.UserName + " words";
            }
            else
            {
                PageHeader = "No words to share!";
            }
        }
    }
}
