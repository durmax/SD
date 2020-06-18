using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SD.Client.Models;
using Microsoft.JSInterop;
using SD.Shared;
using System;

namespace SD.Client.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        //[Inject]
        //IJSRuntime JSRuntime { get; set; }

        [Inject]
        WordService WordService { get; set; }

        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Inject]
        public UserService UserService { set; get; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }


        private string fLang;
        private string tLang;

        protected string FLangCode //{ get; set; }
        {
            get { return fLang; }
            set
            {
                if (fLang != value)
                {
                    fLang = value;
                    LocalStorageService.SetItemAsync("FLang", fLang);
                }
            }
        }

        protected string TLangCode // { get; set; }
        {
            get { return tLang; }
            set
            {
                if (tLang != value)
                {
                    tLang = value;
                    LocalStorageService.SetItemAsync("TLang", tLang);
                }
            }
        }


        protected string UserId { get; set; }
        protected string Word { get; set; }

        protected string Explain { get; set; }

        protected void Reverse()
        {
            string l = FLangCode;
            FLangCode = TLangCode;
            TLangCode = l;
        }


        [Parameter] public List<LangCode> LangCodes { get; set; }
        protected async Task<IEnumerable<LangCode>> SearchLangs(string searchText)
        {
            return await Task.FromResult(LangCodes.Where(x => x.Value.ToLower().Contains(searchText.ToLower())).ToList());
        }

        protected async Task AddWord()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                var user = (await authenticationStateTask).User;

                if (user.Identity.IsAuthenticated)
                {
                    UserId = user.FindFirst(c => c.Type == "tid")?.Value;
                }
                else
                {
                    NavigationManager.NavigateTo("authentication/login");
                }
            }
            else
            {
                WordModel w = new WordModel
                {
                    WordId = Guid.NewGuid().ToString(),
                    Title = Word,
                    WordLang = FLangCode,
                    ToLang = TLangCode,
                    UserId = UserId,
                    CreatedAt = DateTime.Now,
                    Explain = Explain
                };
                if (!string.IsNullOrWhiteSpace(w.Title))
                {
                    await WordService.AddWord(w);
                }
            }
        }


        protected override async Task OnInitializedAsync()
        {
            try
            {
                var user = (await authenticationStateTask).User;

                if (user.Identity.IsAuthenticated)
                {
                    UserId = user.FindFirst(c => c.Type == "oid")?.Value;
                    UserModel userModel = new UserModel()
                        {
                            UserId = UserId,
                            Email = user.FindFirst(c => c.Type == "email")?.Value,
                            Name = user.Identity.Name//user.FindFirst(c => c.Type == ClaimTypes.Surname)?.Value
                        };
                        await UserService.AddUser(userModel);
                }
            }
            catch
            {
                //NavigationManager.NavigateTo("/");
            }

            FLangCode = await LocalStorageService.GetItemAsync<string>("FLang");

            TLangCode = await LocalStorageService.GetItemAsync<string>("TLang");


            if (string.IsNullOrWhiteSpace(FLangCode) || FLangCode == "null" || string.IsNullOrWhiteSpace(TLangCode) || TLangCode == "null")
            {
              NavigationManager.NavigateTo("Languages");
            }
        }

    //    protected async override Task OnAfterRenderAsync(bool firstRender)
    //    {
    //        if (firstRender)
    //        {
    //            await JSRuntime.InvokeVoidAsync(
    //"exampleJsFunctions.focusElement", "wordId");
    //        }
    //    }
    }
}
