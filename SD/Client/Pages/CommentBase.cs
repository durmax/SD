using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SD.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class CommentBase : ComponentBase
    {
        [Inject]
        IJSRuntime JsRuntime { set; get; }
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        protected CurrentUser CurrentUser { get; set; }
        [Parameter]
        public string WordId { get; set; }
        [Parameter]
        public string WordUserId { get; set; }

        [Parameter]
        public string CurrentUserName { get; set; }

        protected string CssDelCom { get; set; } = "d-none";
        protected bool IsChanged { get; set; } = false;

        [Parameter]
        public CommentModel CommentModel { get; set; }

        [Parameter]
        public EventCallback<CommentModel> OnCommentDelete { get; set; }

        protected string note;
        protected bool loading;

        protected int Rows = 1;

        string _myText;

        protected string MyText
        {
            get => _myText;
            set
            {
                _myText = value;
                CommentModel.CommentText = value;
                CalculateSize(value);
            }
        }
        private void CalculateSize(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Rows = Math.Max(value.Split('\n').Length, value.Split('\r').Length);
                Rows = Math.Max(Rows, 1);
                Rows = Math.Min(Rows, 20);
            }
        }

        protected async Task SaveComment()
        {
            IsChanged = false;
            loading = true;
            if (string.IsNullOrEmpty(CurrentUser.id))
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                if (CommentModel.CommentId == null)
                {
                    CommentModel.CommentId = Guid.NewGuid().ToString();
                    CommentModel.CreatedAt = DateTime.Now;
                }

                if (CommentModel.UserId == CurrentUser.id)
                {
                    CommentModel.CommentText = MyText;
                    CommentModel.CommentOwnerName = CurrentUserName;
                    HttpResponseMessage respons = await CurrentUser.httpClient.PostAsJsonAsync($"api/Comment/SaveComment/{WordId}", CommentModel);
                    if (!respons.IsSuccessStatusCode)
                    {
                        IsChanged = false;
                        note = "Comment is saved";
                    }
                }
                else
                {
                    note = "Not Saved, Not Allowed";
                }
            }
            loading = false;
        }

        protected async Task RemoveComment()
        {
            bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", "You try to delete comment, are you sure?");
            if (confirmed)
            {
                if (string.IsNullOrWhiteSpace(CommentModel.CommentId))
                {
                    await OnCommentDelete.InvokeAsync(CommentModel);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(CurrentUser.id) && !string.IsNullOrWhiteSpace(WordId) && !string.IsNullOrWhiteSpace(CommentModel.CommentId) && !string.IsNullOrWhiteSpace(CommentModel.UserId) && (CurrentUser.id == CommentModel.UserId || CurrentUser.id == WordUserId))
                    {
                        await CurrentUser.httpClient.DeleteAsync($"api/Comment/DeleteComment/{CurrentUser.id}/{WordId}/{CommentModel.CommentId}");
                        await OnCommentDelete.InvokeAsync(CommentModel);
                    }
                }
            }
        }

        protected override void OnParametersSet()
        {
            MyText = CommentModel.CommentText;
        }

        protected override void OnInitialized()
        {
            if (CommentModel.UserId == CurrentUser.id || WordUserId == CurrentUser.id)
            {
                CssDelCom = null;
            }
        }
    }
}

