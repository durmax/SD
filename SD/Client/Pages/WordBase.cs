using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class WordBase : ComponentBase
    {
        [Inject]
        IJSRuntime JsRuntime { set; get; }
        [Inject]
        OtherPageService OtherPageService { set; get; }
        [Inject]
        protected CurrentUserService CurrentUser { set; get; }
        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { set; get; }
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        KnownLangsService KnownLangsService { get; set; }
        [Parameter]
        public bool Collapsed { set; get; } //= true;    // hide by default
        public bool CollapsedComm { set; get; } = true;
        public bool CollapsedLike { set; get; } = true;

        [Parameter]
        public string WordId { get; set; }

        [Inject]
        public WordService WordService { set; get; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        public DefaultLangsService DefaultLangsService { get; set; }

        [Parameter]
        public WordDto WordDto { get; set; }

        protected List<string> KnownLangs { get; set; }

        protected string ShareWithImageSRC { get; set; }
        protected string cssClassDelete;// = "d-none";

        [Parameter]
        public string UserId { get; set; }

        protected string cssClassUpdate = "d-none";

        protected bool loading;
        protected string note;
        public int? LikesCount { get; set; }
        protected bool CULiked { get; set; } = false;
        protected IEnumerable<UserRelationshipsWithOneUserDto> likedUsers;

        protected IEnumerable<string> SameWords { get; set; }
        protected IEnumerable<string> LanguageToolWords { get; set; }
        private string FavSite { get; set; }

        protected List<CommentModel> WordComments { get; set; }

        protected WordDto foundWordDtoToUpdate;

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
        protected async Task KeyupAsync(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(FavSite) && WordDto != null)
            {
                string url = OtherPageService.BuildLink(FavSite, WordDto.Title, WordDto.WordLang, WordDto.ToLang);

                await JsRuntime.InvokeVoidAsync("window.open", url, "popup");
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
            (WordDto.ToLang, WordDto.WordLang) = (WordDto.WordLang, WordDto.ToLang);
        }

        [Parameter]
        public EventCallback<WordDto> OnWordSave { get; set; }
        [Parameter]
        public EventCallback<WordDto> OnWordFound { get; set; }

        [Parameter]
        public EventCallback<WordDto> OnWordDelete { get; set; }

        protected async Task OnSelectedAsync(int selection)
        {
            WordDto.ShareWith = (ShareWith)selection;
            ShareWithImageSRC = "/icons/Save" + WordDto.ShareWith.ToString() + ".svg";
            await AddWord();
        }

        protected async Task AddWord()
        {
            loading = true;
            if (CurrentUser.IsAuthenticated)
            {
                WordDto.Explain = MyText;

                HttpResponseMessage respons = await WordService.AddWord(WordDto);

                if (!respons.IsSuccessStatusCode)
                {
                    if ((int)respons.StatusCode == 302)
                    {
                        cssClassUpdate = null;
                        foundWordDtoToUpdate = JsonConvert.DeserializeObject <WordDto>(await respons.Content.ReadAsStringAsync());
                    }
                    //note = await respons.Content.ReadAsStringAsync();
                }
                else
                {
                    if ((int)respons.StatusCode == 200)
                    {
                        note = $"{WordDto.Title} is Saved";

                        WordDto.WordId = foundWordDtoToUpdate.WordId;
                        await OnWordSave.InvokeAsync();
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
            if (CurrentUser.IsAuthenticated)
            {
                loading = true;
                if (foundWordDtoToUpdate != null)
                {
                    WordDto.WordId = foundWordDtoToUpdate.WordId;
                    WordDto.UserId = foundWordDtoToUpdate.UserId;
                    cssClassUpdate = "d-none";

                    HttpResponseMessage respons = await WordService.UpdateWord(WordDto);
                    if (!respons.IsSuccessStatusCode)
                    {
                        //note = $"Sorry, {wordModel.Title} did not updated!";
                        note = await respons.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        note = $"{WordDto.Title} is Updated";
                        await OnWordSave.InvokeAsync();
                    }
                    foundWordDtoToUpdate = null;
                }
                loading = false;
            }
            else //note = "please Login to save word to your account";
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
        }

        protected async Task NewWordAsync(string wordId)
        {
            WordDto = new()
            {
                WordId = wordId,
                WordLang = DefaultLangsService.DefaultWordLang,
                ToLang = DefaultLangsService.DefaultToLang,
                ShareWith = WordDto?.ShareWith ?? ShareWith.Public,
                Explain = null
            };
            MyText = null;
            SetMyText();
            ShareWithImageSRC = "/icons/Save" + WordDto.ShareWith.ToString() + ".svg";

            if (string.IsNullOrWhiteSpace(WordDto.WordLang)) WordDto.WordLang = DefaultLangsService.DefaultWordLang;
            if (string.IsNullOrWhiteSpace(WordDto.ToLang)) WordDto.ToLang = DefaultLangsService.DefaultToLang;
        }

        protected async Task WordChangedAsync(string title)
        {
            cssClassUpdate = "d-none";
            note = null;
            WordDto.Title = title.Trim();
            if (title.Length > 2)
            {
                loading = true;
                if (CurrentUser.IsAuthenticated)
                {
                    SameWords = await WordService.GetWordsContainText(title);
                }
                LanguageToolWords = await WordService.GetLanguageToolWords(WordDto.WordLang, title);

                loading = false;
            }
            else
            {
                SameWords = null;
                LanguageToolWords = null;
            }
        }

        protected async Task SetWord(string id)
        {
            if (CurrentUser.IsAuthenticated && !string.IsNullOrEmpty(id))
            {
                var wDto = await WordService.GetWordById(id);
                if (wDto != null) await OnWordFound.InvokeAsync(wDto);
            }
        }

        protected void SetMyText()
        {
            try
            {
                MyText = WordDto?.Explain;
                CalculateSize(MyText);
                Rows = Rows < 3 ? Rows : Rows++;
            }
            catch
            {
            }
        }

        private async Task BuildKnownLangsAsync()
        {

            if (string.IsNullOrWhiteSpace(KnownLangsService.LangsStr))
            {
                KnownLangsService.LangsStr = await LocalStorageService.GetItemAsync<string>("Langs");
            }

            KnownLangsService.AddKnownLang(WordDto.WordLang);
            KnownLangsService.AddKnownLang(WordDto.ToLang);

            KnownLangs ??= new List<string>();
            KnownLangs = KnownLangsService.KnownLangs;
        }

        protected void CreateComment()
        {
            if (!CurrentUser.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                CommentModel commentModel = new();
                WordComments.Add(commentModel);
                WordDto.CommentsCount++;
                CollapsedComm = false;
            }
        }

        protected async Task RemoveCommentHandlerAsync(CommentModel comment)
        {
            loading = true;
            if (!CurrentUser.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                WordComments.Remove(comment);
                var response = await CurrentUser.HttpClient.DeleteAsync($"api/Comment/DeleteComment/{WordDto.WordId}/{comment.CommentId}");
                if (response.IsSuccessStatusCode)
                {
                    WordDto.CommentsCount--;
                    note = $"Comment of {comment.CommentOwnerName} is deleted";
                }
                else note = $"Comment of {comment.CommentOwnerName} is NOT deleted";
            }
            loading = false;
        }

        protected async Task DeleteWord()
        {
            loading = true;
            if (Guid.TryParse(WordDto?.WordId, out Guid result))// is not a new word
            {
                if (CurrentUser.IsAuthenticated)
                {
                    if (CurrentUser.IsAuthenticated)
                    {
                        bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", "You try to delete '" + WordDto.Title + "', are you sure?");
                        if (confirmed)
                        {
                            var response = await WordService.RemoveWord(WordDto.WordId);
                            if (response.IsSuccessStatusCode)
                            {
                                await OnWordDelete.InvokeAsync(WordDto);
                            }
                            else
                            {
                                //note = $"You can NOT delete {wordModel.Title}";
                                await JsRuntime.InvokeVoidAsync("alert", $"You do NOT have a promising to delete '{WordDto.Title}'");
                            }
                        }
                    }
                }
            }
            else
            {
                await OnWordDelete.InvokeAsync(WordDto);
            }
            loading = false;
        }

        protected async Task GetLikedUsers(int? likesCount)
        {
            CollapsedLike = !CollapsedLike;
            if (!CollapsedLike && likesCount != null)
            {
                likedUsers = await WordService.GetLikedUsers(WordDto.WordId);
            }
        }
        protected async Task LikeAsync()
        {
            if (CurrentUser.IsAuthenticated)
            {
                CULiked = !CULiked;
                LikesCount += CULiked ? 1 : -1;
                await WordService.Like(WordDto.WordId);
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
            if (WordComments == null)
            {
                WordComments = new List<CommentModel>();
                if (WordDto?.WordId != null)
                {
                    try
                    {
                        WordComments = string.IsNullOrEmpty(WordDto?.WordId) ? null : (List<CommentModel>)await CurrentUser.HttpClient.GetFromJsonAsync<IEnumerable<CommentModel>>($"api/Comment/GetWordComments/{WordDto?.WordId}");
                        WordComments.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));
                    }
                    catch { }
                }
            }
        }
        private async Task GetFavLinkAsync()
        {
            string fl = WordDto?.WordLang ?? DefaultLangsService.DefaultWordLang;
            string tl = WordDto?.ToLang ?? DefaultLangsService.DefaultToLang;

            try
            {
                FavSite = await LocalStorageService.GetItemAsync<string>("fav" + "-" + fl + "-" + tl);
            }
            catch { }
        }

        protected override async Task OnInitializedAsync()
        {
            if (Guid.TryParse(WordId, out Guid result))
            {
                try
                {
                    WordDto = await WordService.GetWordById(WordId);
                }
                catch (Exception x)
                {
                    note = x.Message;
                }
            }

            if (WordDto != null && !Guid.TryParse(WordDto.WordId, out Guid res))
            {
                CULiked = WordDto.IsILiked;
                LikesCount = WordDto.LikesCount;
            }
            await BuildKnownLangsAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            ShareWithImageSRC = "/icons/Save" + WordDto.ShareWith.ToString() + ".svg";
            note = null;
            SetMyText();
            await GetFavLinkAsync();
        }
    }
}
