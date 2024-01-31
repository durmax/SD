using Microsoft.AspNetCore.Components;
using SD.Client.Services;
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
        NavigationManager NavigationManager { get; set; }
        [Inject]
        HttpClient HttpClient { get; set; }

        [Parameter]
        public string WordId { get; set; }
        [Parameter]
        public string WordUserId { get; set; }
        [CascadingParameter]
        protected string CurrentUserId { get; set; }
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
            if (string.IsNullOrEmpty(CurrentUserId))
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

                if (CommentModel.UserId == CurrentUserId)
                {
                    CommentModel.CommentText = MyText;
                    CommentModel.CommentOwnerName = CurrentUserName;
                    HttpResponseMessage respons = await HttpClient.PostAsJsonAsync($"api/Comment/SaveComment/{WordId}", CommentModel);
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
            if (string.IsNullOrWhiteSpace(CommentModel.CommentId))
            {
                await OnCommentDelete.InvokeAsync(CommentModel);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(CurrentUserId) && !string.IsNullOrWhiteSpace(WordId) && !string.IsNullOrWhiteSpace(CommentModel.CommentId) && !string.IsNullOrWhiteSpace(CommentModel.UserId) && (CurrentUserId == CommentModel.UserId || CurrentUserId == WordUserId))
                {
                    await HttpClient.DeleteAsync($"api/Comment/DeleteComment/{CurrentUserId}/{WordId}/{CommentModel.CommentId}");
                    await OnCommentDelete.InvokeAsync(CommentModel);
                }
            }
        }

        protected override void OnParametersSet()
        {
            MyText = CommentModel.CommentText;
        }

        protected override void OnInitialized()
        {
            if (CommentModel.UserId == CurrentUserId || WordUserId == CurrentUserId)
            {
                CssDelCom = null;
            }
        }
    }
}

