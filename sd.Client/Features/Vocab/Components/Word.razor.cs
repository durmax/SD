using AKSoftware.Localization.MultiLanguages;
using Blazored.TextEditor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using sd.Client.Contracts;
using sd.Client.Services;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace sd.Client.Features.Vocab.Components;

public class WordBase : ComponentBase, IDisposable
{
    [Inject] ILogger<WordBase> Log { get; set; }
    [Inject] IJSRuntime JsRuntime { set; get; }
    [Inject] DictionaryLinksService DictionaryLinksService { set; get; }
    [Inject] protected CurrentUserService CurrentUser { set; get; }
    [Inject] protected ApiService ApiService { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] ILanguageContainerService LanguageContainer { get; set; }

    [Parameter] public string WordId { get; set; }
    [Parameter] public WordDto WordDto { get; set; }
    [Parameter] public EventCallback<WordDto> OnWordSave { get; set; }
    [Parameter] public EventCallback<WordDto> OnWordFound { get; set; }
    [Parameter] public EventCallback<WordDto> OnWordDelete { get; set; }

    public bool CollapsedComm { set; get; } = true;
    public bool CollapsedLike { set; get; } = true;
    public int? LikesCount { get; set; }
    protected bool CULiked { get; set; } = false;
    protected List<CommentModel> WordComments { get; set; }
    public BlazoredTextEditor QuillHtml { get; set; }
    protected string Explain { get; set; }
    public List<DictionaryProviderDto> dictionaryProviders { get; private set; }

    protected WordSearchField? wordSearchFieldRef;
    protected string cssClassDelete;// = "d-none";
    protected string cssClassUpdate = "d-none";
    protected bool loading;
    protected string note;
    protected bool open = false;

    protected IEnumerable<UserRelationshipsWithOneUserDto> likedUsers;
    protected WordDto foundWordDtoToUpdate;
    protected int Rows = 2;

    protected List<string> ShareVariants { get; set; } = new();

    protected Task OnSpeakingChanged(bool speaking)
    {
        // optional hook
        return Task.CompletedTask;
    }

    protected async Task KeydownAsync(KeyboardEventArgs e)
    {
        // Mobile keyboards may send different keys for the "action" button
        var key = e.Key?.ToLowerInvariant();

        var isSubmit =
            key == "enter" ||
            key == "go" ||
            key == "search" ||
            key == "done";

        dictionaryProviders = await DictionaryLinksService.GetDictionaryProviders(
        WordDto.WordLang, WordDto.ToLang, string.Empty) ?? new List<DictionaryProviderDto>();

        var FavSite = dictionaryProviders?.FirstOrDefault(x => x.IsFavorite)?.Pattern;
        if (isSubmit && !string.IsNullOrWhiteSpace(FavSite) && WordDto is not null)
        {
            var url = DictionaryLinksService.BuildLink(FavSite, WordDto.Title, WordDto.WordLang, WordDto.ToLang);
            await JsRuntime.InvokeVoidAsync("open", url, "_blank");
        }
    }

    protected void Reverse()
    {
        (WordDto.ToLang, WordDto.WordLang) = (WordDto.WordLang, WordDto.ToLang);
    }

    protected void PairChanged(LanguagePair newPair)
    {
        WordDto.WordLang = newPair.From.Code;
        WordDto.ToLang = newPair.To.Code;
    }

    protected async Task OnSelectedAsync(int selection)
    {
        WordDto.ShareWith = (ShareWith)selection;
        await AddWord();
    }

    protected async Task AddWord()
    {
        loading = true;

        if (!CurrentUser.IsAuthenticated)
        {
            NavigationManager.NavigateTo("/authentication/login");
            loading = false;
            return;
        }

        try
        {
            WordDto.Explain = await QuillHtml.GetHTML();

            var response = await ApiService.PostAsync<HttpResponseMessage>("api/Word", WordDto);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                foundWordDtoToUpdate = JsonConvert.DeserializeObject<WordDto>(responseBody);

                note = $"{WordDto.Title} is Saved";
                WordDto.UserId = foundWordDtoToUpdate?.UserId;
                WordDto.WordId = foundWordDtoToUpdate?.WordId;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    await OnWordSave.InvokeAsync(WordDto);
                }
            }
            else
            {
                Console.WriteLine($"Error {response.StatusCode}: {responseBody}");

                if ((int)response.StatusCode == 302)
                {
                    cssClassUpdate = null;
                }
                note = responseBody;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while adding word: {ex.Message}");
            note = "An error occurred while saving the word.";

            Log.LogError(ex.Message);
        }
        finally
        {
            loading = false;
        }

        Log.LogInformation(note);
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

                HttpResponseMessage respons = await ApiService.PutAsync<HttpResponseMessage>($"api/Word", WordDto);
                if (!respons.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Exception occurred while updating word: {respons.Content.ReadAsStringAsync()}");
                    note = "An error occurred while updating the word.";
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
        else
        {
            NavigationManager.NavigateTo("/authentication/login");
        }
    }

    // Simplified: suggestion fetching is handled by WordSearchField component.
    // Keep a lightweight handler for title changes that only updates model state.
    protected Task WordChangedAsync(string title)
    {
        cssClassUpdate = "d-none";
        note = null;
        WordDto ??= new WordDto();
        WordDto.Title = title?.Trim() ?? string.Empty;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // No local cancellation resources to clean up anymore.
    }

    protected async Task SetWord(string id)
    {
        if (CurrentUser.IsAuthenticated && !string.IsNullOrEmpty(id))
        {
            var wDto = await ApiService.GetAsync<WordDto>($"api/Word/{id}");
            if (wDto != null)
            {
                wDto.Score++;
                await ApiService.PutAsync<HttpResponseMessage>($"api/Word", wDto);
                await OnWordFound.InvokeAsync(wDto);
                note = $"{wDto.Title} is found! The new score is {wDto.Score}";
                Log.LogInformation(note);
            }
            else
            {
                note = "The Word is not found!";
                Log.LogInformation(note);
            }
        }
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
                bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", "You try to delete '" + WordDto.Title + "', are you sure?");
                if (confirmed)
                {
                    var response = await ApiService.DeleteAsync($"api/Word/{WordDto.WordId}");
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
        else
        {
            await OnWordDelete.InvokeAsync(WordDto);
        }
        Log.LogInformation($"DeleteWord: {WordDto.Title}");
        loading = false;
    }

    protected async Task GetAi()
    {
        loading = true;
        if (CurrentUser.IsAuthenticated)
        {
            var response = await ApiService.GetStringAsync($"api/Word/GetAI/{WordDto.Title}");

            WordDto.Explain = string.IsNullOrEmpty(WordDto.Explain) ? response : WordDto.Explain += "<br>" + response.Replace("**", "");
            await LoadHtmlExplain();
        }
        await AddWord();
        loading = false;
    }
    protected async Task GetLikedUsers(int? likesCount)
    {
        CollapsedLike = !CollapsedLike;
        if (!CollapsedLike && likesCount != null)
        {
            likedUsers = await ApiService.GetAsync<IEnumerable<UserRelationshipsWithOneUserDto>>($"api/Word/GetLikedUsers/{WordDto.WordId}");
        }
    }
    protected async Task LikeAsync()
    {
        if (CurrentUser.IsAuthenticated)
        {
            CULiked = !CULiked;
            LikesCount += CULiked ? 1 : -1;
            await ApiService.GetAsync<int>($"api/Word/Like/{WordDto.WordId}");
        }
        else
        {
            NavigationManager.NavigateTo("authentication/login");
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
                catch (Exception ex)
                {
                    Log.LogError(ex.Message);
                }
            }
        }
    }

    protected async Task OpenAsync()
    {
        open = !open;
        if (open)
        {
            await LoadHtmlExplain();
        }
    }
    private async Task LoadHtmlExplain()
    {
        if (string.IsNullOrEmpty(WordDto?.Explain))
            return;

        await Task.Delay(100); // give Quill JS time to initialize

        try
        {
            await QuillHtml.LoadHTMLContent(WordDto?.Explain);
            Explain = await QuillHtml.GetText();
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to load Explain: " + ex.Message);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (Guid.TryParse(WordId, out Guid result))
        {
            try
            {
                WordDto = await ApiService.GetAsync<WordDto>($"api/Word/{WordId}");
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
            }
        }
        WordDto ??= new WordDto();
        WordDto.WordLang = WordDto?.WordLang ?? "de"; // ToDo
        WordDto.ToLang = WordDto?.ToLang ?? "ar"; // ToDo

        if (WordDto != null && Guid.TryParse(WordDto.WordId, out Guid res))
        {
            CULiked = WordDto.IsILiked;
            LikesCount = WordDto.LikesCount;
        }

        // Initialize the list here, after LanguageContainer is available
        ShareVariants = new List<string>
        {
            LanguageContainer.Keys["OnlyMe"],
            LanguageContainer.Keys["Friends"],
            LanguageContainer.Keys["Public"]
        };
    }
}
