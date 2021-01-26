using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using SD.Shared;
using System;
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

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Parameter]
        public string UserId { get; set; }
        protected string CurrentUserId { get; set; }

        [Parameter]
        public string UserName { get; set; }
        protected bool Collapsed { set; get; } = true;    // hide by default
        protected bool loading = true;
        protected int currentPage = 0;
        protected string TLang="";
           

        protected List<WordModel> Words { get; set; }

        protected void NewWordHandler(WordModel newWord)
        {
            Words.Add(newWord);
        }
        protected void DeleteWordHandler(WordModel word)
        {
            Words.Remove(word);
        }

        protected async Task Auth()
        {
            var user = (await authenticationStateTask).User;

            if (user.Identity.IsAuthenticated)
            {
                CurrentUserId = user.FindFirst(c => c.Type == "oid")?.Value;
            }
            else
            {
                CurrentUserId = "0";
            }
        }
        protected async Task InitAsync()
        {
            TLang= DefaultLangsService.DefaultToLang;
            await Auth();
            try
            {
                if (string.IsNullOrWhiteSpace(UserId))
                {
                    if (CurrentUserId == "0")
                    {
                        NavigationManager.NavigateTo("/");
                    }
                    else
                    {
                        UserId = CurrentUserId;
                    }
                }
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
            //var word = await WordService.GetWords(CurrentUserId, UserId, TLang, 10, currentPage);
            //currentPage = word.Item1;
            //Words.AddRange(word.Item2);

            Words.AddRange(await WordService.GetAllWords(CurrentUserId, UserId, 10, currentPage));
            loading = false;
        }

        protected override async Task OnParametersSetAsync()
        {
            Words = null;
            await InitAsync();
        }
    }
}
