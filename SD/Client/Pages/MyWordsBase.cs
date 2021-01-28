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
        NavigationManager NavigationManager { get; set; }

        [Inject]
        DefaultLangsService DefaultLangsService { get; set; }
        [Inject]
        public CurrentUserService CurrUsrService { get; set; }

        [Parameter]
        public string UserId { get; set; }
        [Parameter]
        public string currUsrId { get; set; }

        [Parameter]
        public string UserName { get; set; }
        protected bool Collapsed { set; get; } = true;    // hide by default
        protected bool loading = true;
        protected int currentPage = 0;
        protected string TLang;


        protected List<WordModel> Words { get; set; }

        protected void NewWordHandler(WordModel newWord)
        {
            Words.Add(newWord);
        }
        protected void DeleteWordHandler(WordModel word)
        {
            Words.Remove(word);
        }

        protected async Task InitAsync()
        {
            if (await CurrUsrService.IsAuth())
                currUsrId = await CurrUsrService.GetCurrUsrId();
            else
                NavigationManager.NavigateTo("/");

            TLang = DefaultLangsService.DefaultToLang;
            
            try
            {
                await GetNextPage();
            }
            catch
            {
                NavigationManager.NavigateTo("/");
            }
        }

        protected async Task GetNextPage()
        {
            loading = true;
            currentPage++;

            if (Words == null)
            {
                Words = new List<WordModel>();
            }
            var res = await WordService.GetPageWordsFromUserID(currUsrId, UserId, 10, currentPage);
            if (res != null)
            {
                currentPage = res.Item1;
                Words.AddRange(res.Item2);
            }
            loading = false;
        }

        protected override async Task OnParametersSetAsync()
        {
            Words = null;
            await InitAsync();
        }
    }
}
