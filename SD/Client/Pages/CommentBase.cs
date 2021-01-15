using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class CommentBase : ComponentBase
    {
        [Inject]
        public CommentService CommentService { set; get; }
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Parameter]
        public string WordId { get; set; }
        [CascadingParameter]
        protected string CurrentUserId { get; set; }
        protected bool cssClassComment { get; set; } = true;    // hide by default
        [Parameter]
        public string cssClassDisplay { get; set; }           //= "d-none";
        protected bool IsDisabled { get; set; }

        [Parameter]
        public CommentModel commentModel { get; set; }

        protected int Rows = 1;

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
                Rows = Math.Max(Rows, 1);
                Rows = Math.Min(Rows, 20);
            }
        }

        protected async Task SaveComment()
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                if (commentModel.CommentId == null)
                {
                    //commentModel.UserId = CurrentUserId;
                    commentModel.CommentId = Guid.NewGuid().ToString();
                    commentModel.CreatedAt = DateTime.Now;
                }

                if (commentModel.UserId == CurrentUserId)
                {
                    commentModel.CommentText = MyText;
                    HttpResponseMessage respons = await CommentService.SaveComment(commentModel, WordId);
                }
            }
        }

        protected async Task RemoveComment()
        {
            HttpResponseMessage respons = await CommentService.RemoveComment(WordId, commentModel.CommentId);
        }

        protected override void OnParametersSet()
        {
            MyText = commentModel.CommentText;
        }

        protected override void OnInitialized()
        {
            if (commentModel.UserId != CurrentUserId)
            {
                IsDisabled = true;
                cssClassDisplay = "d-none";
            }
        }
    }
}

