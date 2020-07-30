using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class MyWordsBase : ComponentBase
    {
        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        public WordService WordService { set; get; }
        [Inject]
        NavigationManager NavigationManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        protected WordModel wordModel { get; set; } = new WordModel();

        [Parameter]
        public string UserId { get; set; }
        protected string CurrentUserId { get; set; }
        [Parameter]
        public string UserName { get; set; }

        protected bool Collapsed { set; get; } = true;    // hide by default

        //protected bool Collapsed = true;    // hide by default
        protected bool loading;

        protected bool CULiked { get; set; }

        protected string styleDeleted;
        protected string cssClassDelete;// = "d-none";

        protected List<WordModel> Words { get; set; }

        protected void NewWordHandler(WordModel newWord)
        {
            Words.Add(newWord);
        }
        protected async Task DeleteWord()
        {
            loading = true;

            if (!string.IsNullOrWhiteSpace(wordModel.UserId))// is not a new word
            {
                await Auth();
                if (wordModel.UserId == UserId)
                {
                    var response = await WordService.RemoveWord(wordModel.WordId);
                    if (response.IsSuccessStatusCode)
                    {
                      Words.Remove(wordModel);
                    }
                    else
                    {
                        //note = $"You can NOT delete {wordModel.Title}";
                    }
                }
            }
            loading = false;
        }

        protected async Task Auth()
        {
            var user = (await authenticationStateTask).User;

            if (user.Identity.IsAuthenticated)
            {
                CurrentUserId = user.FindFirst(c => c.Type == "oid")?.Value;
                cssClassDelete = null;
            }
            else
            {
                //NavigationManager.NavigateTo("authentication/login");
                CurrentUserId ="0";
            }
        }

        protected override async Task OnInitializedAsync()
        {
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
                else
                {
                    cssClassDelete = UserId == CurrentUserId ? null : "d-none";
                }

                Words = await WordService.GetAllWords(CurrentUserId, UserId);
            }
            catch
            {
                NavigationManager.NavigateTo("/");
            }

            wordModel.WordLang = await LocalStorageService.GetItemAsync<string>("FLang");
            wordModel.ToLang = await LocalStorageService.GetItemAsync<string>("TLang");
        }

        protected override async Task OnParametersSetAsync()
        {
            Words = null;
            await OnInitializedAsync();
        }
    }
}
