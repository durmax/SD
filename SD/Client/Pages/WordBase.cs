using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
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
        IJSRuntime jsRuntime { set; get; }
        [Inject]
        OtherPageService OtherPageService { set; get; }
        [Inject]
        public CurrentUserService CurrUsrService { set; get; }
        [Inject]
        public CurrentUser CurrentUser { set; get; }
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
        public WordDto wordDto { get; set; }

        protected List<string> KnownLangs { get; set; }

        protected string ShareWithClass { get; set; }
        protected string cssClassDelete;// = "d-none";

        [Parameter]
        public string UserId { get; set; }

        protected string cssClassUpdate = "d-none";

        protected bool loading;
        protected string note;

        private string FavSite { get; set; }

        protected List<CommentModel> wordComments { get; set; }

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

        [Inject]
        protected NavigationManager UriHelper { get; set; }
        protected async Task KeyupAsync(KeyboardEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FavSite) && wordDto != null)
            {
                string url = OtherPageService.BuildLink(FavSite, wordDto.Title, wordDto.WordLang, wordDto.ToLang);

                if (e.Key == "Enter")
                {
                    await jsRuntime.InvokeAsync<object>("window.open", url, "popup");
                }
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
            else
            {
                Rows = 2;
            }
        }

        protected void Reverse()
        {
            string l = wordDto.WordLang;
            wordDto.WordLang = wordDto.ToLang;
            wordDto.ToLang = l;
        }

        [Parameter]
        public EventCallback<WordDto> OnWordSave { get; set; }
        [Parameter]
        public EventCallback<WordDto> OnWordFound { get; set; }

        [Parameter]
        public EventCallback<WordDto> OnWordDelete { get; set; }

        protected async Task OnSelectedAsync(int selection)
        {
            wordDto.ShareWith = (ShareWith)selection;
            await AddWord();
        }

        protected async Task AddWord()
        {
            loading = true;
            if (!string.IsNullOrWhiteSpace(CurrentUser.id))
            {
                wordDto.Explain = MyText;

                HttpResponseMessage respons = await WordService.AddWord(wordDto);

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
                    if ((int)respons.StatusCode == 200)
                    {
                        note = $"{wordDto.Title} is Saved";
                        wordDto.WordId = await respons.Content.ReadAsStringAsync();
                        await OnWordSave.InvokeAsync(wordDto);
                        await NewWordAsync();
                    }
                }
            }
            else //note = "please Login to save word to your account";
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            SameWords = null;
            loading = false;
        }

        protected async Task UpdateWord()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUser.id))
            {
                if (string.IsNullOrWhiteSpace(wordDto.UserId) || CurrentUser.id != wordDto.UserId)
                {
                    await AddWord();
                }
                else
                {
                    loading = true;
                    if (!string.IsNullOrWhiteSpace(foundWordIdToUpdate))
                    {
                        wordDto.WordId = foundWordIdToUpdate;
                        cssClassUpdate = "d-none";

                        HttpResponseMessage respons = await WordService.UpdateWord(wordDto);
                        if (!respons.IsSuccessStatusCode)
                        {
                            //note = $"Sorry, {wordModel.Title} did not updated!";
                            note = await respons.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            note = $"{wordDto.Title} is Updated";
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
            wordDto = new WordDto
            {
                WordLang = DefaultLangsService.DefaultWordLang,
                ToLang = DefaultLangsService.DefaultToLang,
                UserId = CurrentUser.id,
                ShareWith = wordDto?.ShareWith ?? ShareWith.Public 
            };
            wordDto.Explain = null;
            MyText = null;
            SetMyText();
            ShareWithClass = "/icons/Save" + wordDto.ShareWith.ToString();

            if (string.IsNullOrWhiteSpace(wordDto.WordLang) || string.IsNullOrWhiteSpace(wordDto.ToLang))
            {
                await SetLangsAsync();
            }
        }

        protected async Task WordChangedAsync(string title)
        {
            cssClassUpdate = "d-none";
            note = null;
            wordDto.Title = title.Trim();
            if (title.Length > 2)
            {
                SameWords = await WordService.GetWordsContainText(CurrentUser.id, title);
            }
            else SameWords = null;
        }

        protected async Task SetWord(string title)
        {
            var wDto = await WordService.GetWordByText(CurrentUser.id, title);
            await OnWordFound.InvokeAsync(wDto);
        }

        protected void SetMyText()
        {
            try
            {
                MyText = wordDto?.Explain;
                CalculateSize(MyText);
                Rows = Rows < 3 ? Rows : Rows++;
            }
            catch
            {
            }
        }

        private async Task SetLangsAsync()
        {
            DefaultLangsService.DefaultWordLang = await LocalStorageService.GetItemAsync<string>("FLang");
            DefaultLangsService.DefaultToLang = await LocalStorageService.GetItemAsync<string>("TLang");

            wordDto.WordLang = DefaultLangsService.DefaultWordLang;
            wordDto.ToLang = DefaultLangsService.DefaultToLang;

            if (string.IsNullOrWhiteSpace(wordDto.WordLang) || wordDto.WordLang == "null" || string.IsNullOrWhiteSpace(wordDto.ToLang) || wordDto.ToLang == "null")
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

            KnownLangsService.GetLangsFromLocalAsync(wordDto.WordLang, wordDto.ToLang);
            KnownLangs ??= new List<string>();
            KnownLangs = KnownLangsService.KnownLangs;
        }

        protected void CreateComment()
        {
            if (string.IsNullOrEmpty(CurrentUser.id))
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                CommentModel commentModel = new CommentModel();
                commentModel.UserId = CurrentUser.id;
                wordComments.Add(commentModel);
                wordDto.CommentsCount++;
                CollapsedComm = false;
            }
        }

        protected async Task RemoveCommentHandlerAsync(CommentModel comment)
        {
            loading = true;
            wordComments.Remove(comment);
            var response = await CommentService.RemoveComment(CurrentUser.id, wordDto.WordId, comment.CommentId);
            if (response.IsSuccessStatusCode)
            {
                wordDto.CommentsCount--;
                note = $"Comment of {comment.CommentOwnerName} is deleted";
            }
            else note = $"Comment of {comment.CommentOwnerName} is NOT deleted";

            loading = false;
        }

        protected async Task DeleteWord()
        {
            loading = true;

            if (!string.IsNullOrWhiteSpace(wordDto.UserId))// is not a new word
            {
                if (wordDto.UserId == CurrentUser.id)
                {
                    var response = await WordService.RemoveWord(wordDto.WordId);
                    if (response.IsSuccessStatusCode)
                    {
                        await OnWordDelete.InvokeAsync(wordDto);
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
        protected IEnumerable<UserRelationshipsWithOneUserDto> likedUsers;
        protected IEnumerable<string> SameWords { get; set; }

        protected async Task GetLikedUsers(int? likesCount)
        {
            CollapsedLike = !CollapsedLike;
            if (!CollapsedLike && likesCount != null)
            {
                likedUsers = await WordService.GetLikedUsers(CurrentUser.id, wordDto.WordId);
            }
        }
        protected async Task LikeAsync()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUser.id) && CurrentUser.id != "0")
            {
                CULiked = !CULiked;
                LikesCount += CULiked ? 1 : -1;
                await WordService.Like(CurrentUser.id, wordDto.WordId);
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
            }
        }

        protected async Task GetWordCommentsAsync()
        {
            CollapsedComm = !CollapsedComm;
            if (wordComments == null)
            {
                wordComments = new List<CommentModel>();
                if (wordDto?.WordId != null)
                {
                    try
                    {
                        wordComments = (List<CommentModel>)await CommentService.GetWordComments(wordDto?.WordId);
                        wordComments.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));
                    }
                    catch { }
                }
            }
        }
        private async Task GetFavLinkAsync()
        {
            string fl;
            string tl;

            if (wordDto != null)
            {
                fl = wordDto.WordLang;
                tl = wordDto.ToLang;
            }
            else
            {
                fl = DefaultLangsService.DefaultWordLang;
                tl = DefaultLangsService.DefaultToLang;
            }
            try
            {
                FavSite = await LocalStorageService.GetItemAsync<string>("fav" + "-" + fl + "-" + tl);
            }
            catch { }
        }

        protected override async Task OnInitializedAsync()
        {
            if (wordDto == null || string.IsNullOrWhiteSpace(wordDto.WordId))
            {
                await NewWordAsync();
            }
            else
            {
                CULiked = wordDto.IsILiked;
                LikesCount = wordDto.LikesCount;
            }

            await BuildKnownLangsAsync();
        }

        protected override async Task OnParametersSetAsync()
        { 
            ShareWithClass = "/icons/Save" + wordDto.ShareWith.ToString() + ".svg";
            note = null;
            SetMyText();
            await GetFavLinkAsync();
        }
    }
}
