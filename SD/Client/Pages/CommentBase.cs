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
        protected bool IsChanged { get; set; } =false;
        protected bool IsDisabled { get; set; }

        [Parameter]
        public CommentModel commentModel { get; set; }

        [Parameter]
        public EventCallback<CommentModel> OnCommentDelete { get; set; }

        protected bool loading;

        protected int Rows = 1;

        string _myText;

        protected string MyText
        {
            get => _myText;
            set
            {   
                _myText = value;
                commentModel.CommentText = value;
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
            loading = true;
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                if (commentModel.CommentId == null)
                {
                    commentModel.CommentId = Guid.NewGuid().ToString();
                    commentModel.CreatedAt = DateTime.Now;
                }

                if (commentModel.UserId == CurrentUserId)
                {
                    commentModel.CommentText = MyText;
                    HttpResponseMessage respons = await CommentService.SaveComment(commentModel, WordId);
                }
            }
            IsChanged = false;
            loading = false;
        }

        protected async Task RemoveComment()
        {
            loading = true;
            if (string.IsNullOrWhiteSpace(commentModel.CommentId))
            {
               // await OnCommentDelete.InvokeAsync(commentModel);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(CurrentUserId))
                {
                    if (!string.IsNullOrWhiteSpace(commentModel.UserId) && CurrentUserId == commentModel.UserId)
                    {
                        HttpResponseMessage respons = await CommentService.RemoveComment(WordId, commentModel.CommentId);
                        if (respons.IsSuccessStatusCode)
                        {
                          await OnCommentDelete.InvokeAsync(commentModel);  
                        }
                    }
                }
            }
            loading = false;
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

