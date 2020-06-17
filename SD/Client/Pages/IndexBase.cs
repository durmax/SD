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
        protected LangCodeService LangCodeService { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        IJSRuntime JSRuntime { get; set; }

        [Inject]
        WordService WordService { get; set; }

        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Inject]
        public UserService UserService { set; get; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }



        protected string UserId { get; set; }

        private LangCode SFL;
        private LangCode STL;

        protected LangCode SelectedFL
        {
            get { return SFL; }
            set
            {
                SFL = value;
                LocalStorageService.SetItemAsync("FLang", SelectedFL.Key);
            }
        }

        protected LangCode SelectedTL
        {
            get { return STL; }
            set
            {
                STL = value;
                LocalStorageService.SetItemAsync("TLang", SelectedTL.Key);
            }
        }


        protected string Word { get; set; }

        protected string Explain { get; set; }

        protected void Reverse()
        {
            LangCode l = SelectedFL;
            SelectedFL = SelectedTL;
            SelectedTL = l;
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
                    WordLang = SelectedFL.Key,
                    ToLang = SelectedTL.Key,
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
                    //UserModel userModel = await UserService.GetUserById(UserId);
                    //if (userModel.UserId == null)
                    //{
                    UserModel userModel = new UserModel()
                        {
                            UserId = UserId,
                            Email = user.FindFirst(c => c.Type == "email")?.Value,
                            Name = user.Identity.Name//user.FindFirst(c => c.Type == ClaimTypes.Surname)?.Value
                        };
                        await UserService.AddUser(userModel);
                   // }
                }
            }
            catch
            {
                //NavigationManager.NavigateTo("/");
            }

            string fl = await LocalStorageService.GetItemAsync<string>("FLang");
            fl = (string.IsNullOrWhiteSpace(fl) || fl == "null") ? "en" : fl;
            string tl = await LocalStorageService.GetItemAsync<string>("TLang");
            tl = (string.IsNullOrWhiteSpace(tl) || tl == "null") ? "de" : tl;

            SelectedFL = new LangCode
            {
                Key = fl,
                Value = LangCodeService.Langs[fl]
            };

            SelectedTL = new LangCode
            {
                Key = tl,
                Value = LangCodeService.Langs[tl]
            };

            LangCodes = new List<LangCode>();
            foreach (var item in LangCodeService.Langs)
            {
                LangCode langCode = new LangCode
                {
                    Key = item.Key,
                    Value = item.Value
                };

                LangCodes.Add(langCode);
            }


        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync(
    "exampleJsFunctions.focusElement", "wordId");
            }
        }
    }
}
