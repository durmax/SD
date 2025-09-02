using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using sd.Client.Services;
using sd.Shared;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace sd.Client.Pages
{
    public class CommentBase : ComponentBase
    {
        [Inject]
        IJSRuntime JsRuntime { set; get; }
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        protected CurrentUserService CurrentUser { get; set; }    
        [Inject]
        protected ApiService ApiService { get; set; }
        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { set; get; }

        [Parameter]
        public string WordId { get; set; }
        [Parameter]
        public string WordUserId { get; set; }

        [Parameter]
        public string CurrentUserName { get; set; }

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
            if (!CurrentUser.IsAuthenticated)
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

                if (CurrentUser.IsAuthenticated)
                {
                    CommentModel.CommentText = MyText;
                    HttpResponseMessage respons = await ApiService.PostAsync<HttpResponseMessage>($"api/Comment/SaveComment/{WordId}", CommentModel);
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
            if (confirmed && CurrentUser.IsAuthenticated)
            {
                if (string.IsNullOrWhiteSpace(CommentModel.CommentId))
                {
                    await OnCommentDelete.InvokeAsync(CommentModel);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(WordId) && !string.IsNullOrWhiteSpace(CommentModel.CommentId) && !string.IsNullOrWhiteSpace(CommentModel.UserId))
                    {
                        await ApiService.DeleteAsync($"api/Comment/DeleteComment/{WordId}/{CommentModel.CommentId}");
                        await OnCommentDelete.InvokeAsync(CommentModel);
                    }
                }
            }
        }

        protected override void OnParametersSet()
        {
            MyText = CommentModel.CommentText;
        }
    }
}

