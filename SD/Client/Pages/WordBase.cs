using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class WordBase : ComponentBase
    {
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Parameter]
        public bool Collapsed { set; get; } = true;    // hide by default

        protected bool opCollapsed { set; get; } = false;    // show by default

        [Inject]
        public WordService WordService { set; get; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        protected List<string> KnownLangs { get; set; }
        protected string LangsStr { get; set; }
        protected string ShareWithClass { get; set; } = "fa-user-lock";

        protected bool ShareWithCollapsed = true;

        [Parameter]
        public WordModel wordModel { get; set; }
        //[Parameter]
        //public string WordId { get; set; }

        [Parameter]
        public string UserId { get; set; }
        private string CurrentUserId { get; set; }

        protected string cssClassUpdate = "d-none";
        protected bool cssClassComment { get; set; } = true;    // hide by default
        protected bool loading;
        protected string note;

        protected string foundWordIdToUpdate;

        protected int Rows = 2;

        string _myText;
        protected string MyText
        {
            get => _myText;
            set
            {
                _myText = value;
                CalculateSize(value);
            }
        }

        private void CalculateSize(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Rows = Math.Max(value.Split('\n').Length, value.Split('\r').Length);
                Rows = Math.Max(Rows, 2);
                Rows = Math.Min(Rows, 20);
            }
        }

        protected void Reverse()
        {
            string l = wordModel.WordLang;
            wordModel.WordLang = wordModel.ToLang;
            wordModel.ToLang = l;
        }

        [Parameter]
        public EventCallback<WordModel> OnWordSave { get; set; }

        protected async Task AddWord()
        {
            loading = true;
            if (!string.IsNullOrWhiteSpace(CurrentUserId))
            {
                if (string.IsNullOrWhiteSpace(wordModel.UserId) || CurrentUserId != wordModel.UserId)
                {
                    wordModel.UserId = CurrentUserId;
                    wordModel.WordId = Guid.NewGuid().ToString();
                    wordModel.CreatedAt = DateTime.Now;

                    wordModel.Likes = null;
                }
                wordModel.Explain = MyText;

                HttpResponseMessage respons = await WordService.AddWord(wordModel);

                if (!respons.IsSuccessStatusCode)
                {
                    if ((int)respons.StatusCode == 302)
                    {
                        cssClassUpdate = "";
                        foundWordIdToUpdate = await respons.Content.ReadAsStringAsync();
                    }
                    //note = await respons.Content.ReadAsStringAsync();
                }
                else
                {
                    note = $"{wordModel.Title} is Saved";
                    if ((int)respons.StatusCode == 200)
                    {
                        if (CurrentUserId == UserId)
                        {
                            await OnWordSave.InvokeAsync(wordModel);
                        }

                        NewWord();
                    }
                }
            }
            else //note = "please Login to save word to your account";
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            loading = false;
        }

        protected async Task UpdateWord()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUserId))
            {
                if (string.IsNullOrWhiteSpace(wordModel.UserId) || CurrentUserId != wordModel.UserId)
                {
                    await AddWord();
                }
                else
                {
                    loading = true;
                    if (!string.IsNullOrWhiteSpace(foundWordIdToUpdate))
                    {
                        wordModel.WordId = foundWordIdToUpdate;
                        cssClassUpdate = "d-none";
                        HttpResponseMessage respons = await WordService.UpdateWord(wordModel);
                        if (!respons.IsSuccessStatusCode)
                        {
                            //note = $"Sorry, {wordModel.Title} did not updated!";
                            note = await respons.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            note = $"{wordModel.Title} is Updated";
                            //NewWord();
                            //MyText = "";
                        }
                        foundWordIdToUpdate = "";
                    }
                    loading = false;
                }
            }
            else //note = "please Login to save word to your account";
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
        }

        protected void NewWord()
        {
            string wl = wordModel.WordLang;
            string tl = wordModel.ToLang;
            wordModel = new WordModel
            {
                WordId = Guid.NewGuid().ToString(),
                WordLang = wl,
                ToLang = tl,
                UserId = CurrentUserId,
                CreatedAt = DateTime.Now
            };
            MyText = null;
        }

        protected void WordChanged(string title)
        {
            //opCollapsed = false;
            note = "";
            wordModel.Title = title;
        }

        private void SetMyText()
        {
            if (string.IsNullOrWhiteSpace(wordModel.Explain))
            {
                MyText = "";
                cssClassComment = true;
            }
            else
            {
                MyText = wordModel.Explain;
                CalculateSize(MyText);
                Rows = Rows < 3 ? Rows : Rows++;
                cssClassComment = false;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var user = (await authenticationStateTask).User;
            if (user.Identity.IsAuthenticated)
            {
                CurrentUserId = user.FindFirst(c => c.Type == "oid")?.Value;
            }

            if (wordModel == null)
            {
                wordModel = new WordModel();
            }

            //try
            //{      //  WordId fom URL
            //    if (!string.IsNullOrWhiteSpace(WordId))
            //    {
            //        wordModel = await WordService.GetWordById(WordId);
            //        Collapsed = false;
            //    }
            //}
            //catch { }

            SetMyText();

            KnownLangs = new List<string>();
            KnownLangs.Add(await LocalStorageService.GetItemAsync<string>("FLang"));
            KnownLangs.Add(await LocalStorageService.GetItemAsync<string>("TLang"));

            LangsStr = await LocalStorageService.GetItemAsync<string>("Langs");
            if (!string.IsNullOrWhiteSpace(LangsStr))
            {
                string[] langArray = LangsStr.Split(",");

                foreach (var lan in langArray)
                {
                    if (!string.IsNullOrWhiteSpace(lan))
                    {
                        if (!KnownLangs.Contains(lan))
                        {
                            KnownLangs.Add(lan);
                        }
                    }
                }
            }


        }
        protected override void OnParametersSet()
        {
            note = null;

            SetMyText();

            switch (wordModel.ShareWith)
            {
                case 0:
                    ShareWithClass = "/icons/user-lock-solid.svg";
                    break;
                case 1:
                    ShareWithClass = "/icons/user-friends-solid.svg";
                    break;
                case 2:
                    ShareWithClass = "/icons/globe-solid.svg";
                    break;
            }
        }
    }
}
