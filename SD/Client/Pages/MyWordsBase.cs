using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class MyWordsBase : ComponentBase
    {
        [Inject]
        public WordService WordService { set; get; }
        [Inject]
        NavigationManager NavigationManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected bool Collapsed = true;    // hide by default

        protected string UserId { get; set; }
        protected List<WordModel> Words { get; set; }

        protected async Task<List<WordModel>> GetWords()
        {
            await Auth();
            return await WordService.GetAllWords(UserId);
        }

        protected async Task Auth()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                var user = (await authenticationStateTask).User;

                if (user.Identity.IsAuthenticated)
                {
                    UserId = user.FindFirst(c => c.Type == "oid")?.Value;
                }
                else
                {
                    NavigationManager.NavigateTo("authentication/login");
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Words = await GetWords();
            }
            catch
            {

            }

        }
    }
}
