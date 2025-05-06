using Blazored.TextEditor;
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
        LocalStorageAccessor LocalStorageAccessor { get; set; }
        [Inject]
        IJSRuntime JsRuntime { set; get; }
        [Inject]
        OtherPageService OtherPageService { set; get; }
        [Inject]
        protected CurrentUserService CurrentUser { set; get; }
        [Inject]
        protected ApiService ApiService { get; set; }
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

        public BlazoredTextEditor QuillHtml { get; set; }

        protected async Task KeyupAsync(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(FavSite) && WordDto != null)
            {
                string url = OtherPageService.BuildLink(FavSite, WordDto.Title, WordDto.WordLang, WordDto.ToLang);

                await JsRuntime.InvokeVoidAsync("window.open", url, "popup");
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
            ShareWithImageSRC = $"/icons/Save{WordDto.ShareWith.ToString()}.svg";
            await AddWord();
        }

        protected async Task AddWord()
        {
            loading = true;
            if (CurrentUser.IsAuthenticated)
            {
                WordDto.Explain = await QuillHtml.GetHTML();

                HttpResponseMessage response = await WordService.AddWord(WordDto);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    foundWordDtoToUpdate = JsonConvert.DeserializeObject<WordDto>(json);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error {response.StatusCode}: {error}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 302)
                    {
                        cssClassUpdate = null;
                    }
                    //note = await respons.Content.ReadAsStringAsync();
                }
                else
                {
                    if ((int)response.StatusCode == 200)
                    {
                        note = $"{WordDto.Title} is Saved";
                        WordDto.UserId = foundWordDtoToUpdate.UserId;
                        WordDto.WordId = foundWordDtoToUpdate?.WordId;
                        await OnWordSave.InvokeAsync(WordDto);
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
                    WordDto.Explain = await QuillHtml.GetHTML();
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
                        await OnWordSave.InvokeAsync(WordDto);
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

        protected async Task WordChangedAsync(string title)
        {
            cssClassUpdate = "d-none";
            note = null;
            WordDto.Title = title.Trim();

            if (title.Length > 2)
            {
                loading = true;

                Task<List<string>> wordsTask = null;
                Task<List<string>> languageToolWordsTask = null;

                if (CurrentUser.IsAuthenticated)
                {
                    // Start SameWords task
                    wordsTask = WordService.GetWordsContainText(title);
                    // Continue without waiting for SameWords to complete
                }

                // Start LanguageToolWords task
                languageToolWordsTask = WordService.GetLanguageToolWords(WordDto.WordLang, title);

                // Use continuations to update UI as each task completes
                if (wordsTask != null)
                {
                    _ = wordsTask.ContinueWith(async task =>
                    {
                        SameWords = await task;
                        // Trigger re-render after SameWords is set
                        StateHasChanged();
                    });
                }

                _ = languageToolWordsTask.ContinueWith(async task =>
                {
                    LanguageToolWords = await task;
                    // Trigger re-render after LanguageToolWords is set
                    StateHasChanged();
                });

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
                if (wDto != null)
                {
                    wDto.Score++;
                    await WordService.UpdateWord(wDto);
                    await OnWordFound.InvokeAsync(wDto);
                }
                else
                {
                    note = "The Word is not found!";
                }
            }
        }

        private async Task BuildKnownLangsAsync()
        {

            KnownLangsService.LangsStr = await LocalStorageAccessor.GetValueAsync<string>("Langs");
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
                var response = await ApiService.DeleteAsync($"api/Comment/DeleteComment/{WordDto.WordId}/{comment.CommentId}");
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

        protected async void OnCollapsed()
        {
            Collapsed = !Collapsed;

            if (!Collapsed)
            {
                if (WordDto.WordId != "0")
                {
                    WordDto.Score++;
                    try
                    {
                        await WordService.UpdateWord(WordDto);
                    }
                    catch
                    {
                        WordDto.Score--;
                    }                 
                }
                await LoadHtmlExplain();
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
                        WordComments = string.IsNullOrEmpty(WordDto?.WordId) ? null : (List<CommentModel>)await ApiService.GetAsync<IEnumerable<CommentModel>>($"api/Comment/GetWordComments/{WordDto?.WordId}");
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
                FavSite = await LocalStorageAccessor.GetValueAsync<string>($"fav-{fl}{tl}");
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
            ShareWithImageSRC = $"/icons/Save{WordDto.ShareWith.ToString()}.svg";
            note = null;
            await GetFavLinkAsync();
        }

        public ElementReference ReferenceToInputControl;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await JsRuntime.InvokeVoidAsync("Utility.setFocus", ReferenceToInputControl);
                }
                catch { }

                await LoadHtmlExplain();
            }
        }

        private async Task LoadHtmlExplain()
        {
            int counter = 0;

            if (!Collapsed)
            {
                while (counter < 10)
                {
                    try
                    {
                        await QuillHtml.LoadHTMLContent(WordDto?.Explain);
                        counter = 11;
                    }
                    catch
                    {
                        await Task.Delay(100);
                        counter++;
                    }
                }
            }
        }
    }
}
