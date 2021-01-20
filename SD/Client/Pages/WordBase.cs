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
        [Inject]
        KnownLangsService KnownLangsService { get; set; }
        [Parameter]
        public bool Collapsed { set; get; } = true;    // hide by default
        public bool CollapsedComm { set; get; } = true;

        protected bool opCollapsed { set; get; } = false;    // show by default

        [Inject]
        public WordService WordService { set; get; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        public DefaultLangsService DefaultLangsService { get; set; }
        [Inject]
        public CommentService CommentService { get; set; }

        [Parameter]
        public WordModel wordModel { get; set; }

        protected string LangsStr { get; set; }

        protected List<string> KnownLangs { get; set; }

        protected string ShareWithClass { get; set; }

        protected bool ShareWithCollapsed = true;

        [Parameter]
        public string UserId { get; set; }
        public string CurrentUserId { get; set; }
        public string CurrentUserName { get; set; }

        protected string cssClassUpdate = "d-none";

        [Parameter]
        public bool cssClassComment { get; set; } = true;  // hide by default
        protected bool loading;
        protected string note;
        protected List<CommentModel> wordComments { get; set; } = new List<CommentModel>();

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
                wordModel.Comments.RemoveAll(x => x.CommentId == null);
                wordModel.Comments.RemoveAll(x => x.UserId != CurrentUserId);
                HttpResponseMessage respons = await WordService.AddWord(wordModel);

                if (!respons.IsSuccessStatusCode)
                {
                    if ((int)respons.StatusCode == 302)
                    {
                        cssClassUpdate = null;
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
                        await NewWordAsync();
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
                        }
                        foundWordIdToUpdate = null;
                    }
                    loading = false;
                }
            }
            else //note = "please Login to save word to your account";
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
        }

        protected async Task NewWordAsync()
        {
            wordModel = new WordModel
            {
                WordId = Guid.NewGuid().ToString(),
                WordLang = DefaultLangsService.DefaultWordLang,
                ToLang = DefaultLangsService.DefaultToLang,
                UserId = CurrentUserId,
                CreatedAt = DateTime.Now
            };
            wordModel.Explain = null;
            SetMyText();
            ShareWithClass = "/icons/cloud-upload-alt-solid.svg";

            if (string.IsNullOrWhiteSpace(wordModel.WordLang) || string.IsNullOrWhiteSpace(wordModel.ToLang))
            {
                await SetLangsAsync();
            }
        }

        protected void WordChanged(string title)
        {
            //opCollapsed = false;
            note = null;
            wordModel.Title = title.Trim();
        }

        private void SetMyText()
        {
            if (string.IsNullOrWhiteSpace(wordModel.Explain))
            {
                MyText = null;
                //cssClassComment = true;
            }
            else
            {
                MyText = wordModel.Explain;
                CalculateSize(MyText);
                Rows = Rows < 3 ? Rows : Rows++;
                //cssClassComment = false;
            }
        }

        private async Task SetLangsAsync()
        {
            DefaultLangsService.DefaultWordLang = await LocalStorageService.GetItemAsync<string>("FLang");
            DefaultLangsService.DefaultToLang = await LocalStorageService.GetItemAsync<string>("TLang");

            wordModel.WordLang = DefaultLangsService.DefaultWordLang;
            wordModel.ToLang = DefaultLangsService.DefaultToLang;

            if (string.IsNullOrWhiteSpace(wordModel.WordLang) || wordModel.WordLang == "null" || string.IsNullOrWhiteSpace(wordModel.ToLang) || wordModel.ToLang == "null")
            {
                NavigationManager.NavigateTo("Languages");
            }
        }

        private async Task BuildKnownLangsAsync()
        {
            if (KnownLangsService.KnownLangs.Count == 0)
            {
                LangsStr = await LocalStorageService.GetItemAsync<string>("Langs");

                KnownLangsService.GetLangsFromLocalAsync(LangsStr, wordModel.WordLang, wordModel.ToLang);
            }

            KnownLangs = new List<string>();
            KnownLangs = KnownLangsService.KnownLangs;
        }

        protected void CreateComment()
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                CommentModel commentModel = new CommentModel();
                commentModel = new CommentModel();
                commentModel.UserId = CurrentUserId;
                wordComments.Add(commentModel);
                CollapsedComm = false;
            }
        }

        protected async Task RemoveCommentHandlerAsync(CommentModel comment)
        {
            loading = true;
            wordModel.Comments.Remove(comment);

            HttpResponseMessage respons = await WordService.UpdateWord(wordModel);
            if (!respons.IsSuccessStatusCode)
            {
                note = await respons.Content.ReadAsStringAsync();
            }
            else
            {
                note = $"Comment of {comment.CommentOwnerName} is deleted";
            }
            loading = false;
        }
        protected override async Task OnInitializedAsync()
        {
            var user = (await authenticationStateTask).User;
            if (user.Identity.IsAuthenticated)
            {
                CurrentUserId = user.FindFirst(c => c.Type == "oid")?.Value;
                CurrentUserName = user.Identity.Name;
            }

            if (wordModel == null)
            {
                await NewWordAsync();
            }

            SetMyText();

            await BuildKnownLangsAsync();

            wordModel.ShareWith = 3; // nothing
        }

        protected override void OnParametersSet()
        {
            note = null;

            SetMyText();

            if (wordModel.Comments != null)
            {
                wordComments = wordModel.Comments;
                wordComments.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));
            }

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
                default:
                    ShareWithClass = "/icons/cloud-upload-alt-solid.svg";
                    break;
            }
        }
    }
}
