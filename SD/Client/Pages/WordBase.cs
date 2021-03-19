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

        protected bool ShareWithCollapsed = true;

        protected string cssClassDelete;// = "d-none";

        [Parameter]
        public string UserId { get; set; }

        protected string cssClassUpdate = "d-none";

        [Parameter]
        public bool cssClassComment { get; set; } = false;
        protected bool loading;
        protected string note;
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
        public EventCallback<WordDto> OnWordDelete { get; set; }

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
                        await OnWordSave.InvokeAsync(wordDto);
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
                WordId = Guid.NewGuid().ToString(),
                WordLang = DefaultLangsService.DefaultWordLang,
                ToLang = DefaultLangsService.DefaultToLang,
                UserId = CurrentUser.id
            };
            wordDto.Explain = null;
            MyText = null;
            SetMyText();
            ShareWithClass = "/icons/cloud-upload-alt-solid.svg";

            if (string.IsNullOrWhiteSpace(wordDto.WordLang) || string.IsNullOrWhiteSpace(wordDto.ToLang))
            {
                await SetLangsAsync();
            }
        }

        protected void WordChanged(string title)
        {
            //opCollapsed = false;
            note = null;
            wordDto.Title = title.Trim();
        }

        protected void SetMyText()
        {
            if (!cssClassComment)
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

        protected async Task GetLikedUsers(int? likesCount)
        {
            CollapsedLike = !CollapsedLike;
            if (!CollapsedLike && likesCount != null)
            {
                likedUsers = await WordService.GetLikedUsers(CurrentUser.id, wordDto.WordId);
            }
        }
        protected async Task Like()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUser.id) && CurrentUser.id != "0")
            {
                LikesCount = await WordService.Like(CurrentUser.id, wordDto.WordId);
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
                wordDto.ShareWith = 3; // nothing
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
                    catch {}
                }
            }
        }
        protected override async Task OnInitializedAsync()
        {
            if (wordDto == null)
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

        protected override void OnParametersSet()
        {
            note = null;
            SetMyText();
            switch (wordDto?.ShareWith)
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
