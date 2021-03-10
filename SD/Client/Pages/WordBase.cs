using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
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
        [Inject]
        public CurrentUserService CurrUsrService { set; get; }

        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        KnownLangsService KnownLangsService { get; set; }
        [Parameter]
        public bool Collapsed { set; get; } //= true;    // hide by default
        public bool CollapsedComm { set; get; } = true;
        public bool CollapsedLike { set; get; } = true;

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

        protected List<string> KnownLangs { get; set; }

        protected string ShareWithClass { get; set; }

        protected bool ShareWithCollapsed = true;

        protected string cssClassDelete;// = "d-none";

        [Parameter]
        public string UserId { get; set; }

        protected string cssClassUpdate = "d-none";

        [Parameter]
        public bool cssClassComment { get; set; } = false;
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

        [Parameter]
        public EventCallback<WordModel> OnWordDelete { get; set; }

        protected async Task AddWord()
        {
            loading = true;
            if (!string.IsNullOrWhiteSpace(CurrUsrService.id))
            {
                if (string.IsNullOrWhiteSpace(wordModel.UserId) || CurrUsrService.id != wordModel.UserId)
                {
                    wordModel.UserId = CurrUsrService.id;
                    wordModel.WordId = Guid.NewGuid().ToString();
                    wordModel.CreatedAt = DateTime.Now;
                    wordModel.Likes = null;
                }
                wordModel.Explain = MyText;
                if (wordModel.Comments != null)
                {
                    wordModel.Comments.RemoveAll(x => x.CommentId == null);
                    wordModel.Comments.RemoveAll(x => x.UserId != CurrUsrService.id);
                }

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
                        if (CurrUsrService.id == UserId)
                        {
                            await OnWordSave.InvokeAsync(wordModel);
                            MyText = null;
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
            if (!string.IsNullOrWhiteSpace(CurrUsrService.id))
            {
                if (string.IsNullOrWhiteSpace(wordModel.UserId) || CurrUsrService.id != wordModel.UserId)
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
                UserId = CurrUsrService.id,
                CreatedAt = DateTime.Now
            };
            wordModel.Explain = null;
            //SetMyText();
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

        protected void SetMyText()
        {
            if (!cssClassComment && string.IsNullOrWhiteSpace(MyText))
            {
                try
                {
                    MyText = wordModel?.Explain;
                    CalculateSize(MyText);
                    Rows = Rows < 3 ? Rows : Rows++;
                }
                catch
                {
                }
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

            if (string.IsNullOrWhiteSpace(KnownLangsService.LangsStr))
            {
                KnownLangsService.LangsStr = await LocalStorageService.GetItemAsync<string>("Langs");
            }

            KnownLangsService.GetLangsFromLocalAsync(wordModel.WordLang, wordModel.ToLang);
            KnownLangs ??= new List<string>();
            KnownLangs = KnownLangsService.KnownLangs;
        }

        protected void CreateComment()
        {
            if (string.IsNullOrEmpty(CurrUsrService.id))
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                CommentModel commentModel = new CommentModel();
                commentModel = new CommentModel();
                commentModel.UserId = CurrUsrService.id;
                wordComments.Add(commentModel);
                CollapsedComm = false;
            }
        }

        protected async Task RemoveCommentHandlerAsync(CommentModel comment)
        {
            loading = true;
            wordComments.Remove(comment);
            if (wordModel.Comments != null && wordModel.Comments.Contains(comment))
            {
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
            }
            loading = false;
        }

        protected async Task DeleteWord()
        {
            loading = true;

            if (!string.IsNullOrWhiteSpace(wordModel.UserId))// is not a new word
            {
                if (wordModel.UserId == CurrUsrService.id)
                {
                    var response = await WordService.RemoveWord(wordModel.WordId);
                    if (response.IsSuccessStatusCode)
                    {
                        await OnWordDelete.InvokeAsync(wordModel);
                    }
                    else
                    {
                        //note = $"You can NOT delete {wordModel.Title}";
                    }
                }
            }
            loading = false;
        }

        /// <summary>
        /// Like start
        /// </summary>
        public int? LikesCount { get; set; }
        protected bool CULiked { get; set; } = false;
        protected Dictionary<string, Tuple<string, string>> likedUsers;

        protected async Task GetLikedUsers(int? likesCount)
        {
            if (!CollapsedLike && likesCount != null)
            {
                likedUsers = await WordService.GetLikedUsers(CurrUsrService.id, wordModel.WordId);
            }
        }
        protected async Task Like()
        {
            if (!string.IsNullOrWhiteSpace(CurrUsrService.id) && CurrUsrService.id != "0")
            {
                LikesCount = await WordService.Like(CurrUsrService.id, wordModel.WordId);
                CULiked = !CULiked;
                //CULikeImg = CULiked ? "/icons/thumbs-up-solid.svg" : "/icons/thumbs-up-regular.svg";
            }
            else
            {
                NavigationManager.NavigateTo("authentication/login");
            }
        }

        /// <summary>
        /// Like end
        /// </summary>
        /// <returns></returns>

        protected void OnCollapsed()
        {
            Collapsed = !Collapsed;

            if (!Collapsed)
            {
                SetMyText();
                wordModel.ShareWith = 3; // nothing
            }
        }

        protected override async Task OnInitializedAsync()
        {
            if (wordModel == null)
            {
                await NewWordAsync();
            }
            else
            {
                if (wordModel.Likes != null)
                {
                    CULiked = wordModel.Likes.Contains(CurrUsrService.id);
                    LikesCount = wordModel.Likes.Count;
                }
            }

            await BuildKnownLangsAsync();
        }

        protected override void OnParametersSet()
        {
            note = null;

            SetMyText();

            if (wordModel?.Comments != null)
            {
                wordComments = wordModel.Comments;
                wordComments.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));
            }

            switch (wordModel?.ShareWith)
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
