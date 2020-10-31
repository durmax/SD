using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class CommentBase : ComponentBase
    {
        [Inject]
        public CommentService CommentService { set; get; }
        [Inject]
        public WordService WordService { set; get; }
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Parameter]
        public string CommentID { get; set; }
        [Parameter]
        public string WordId { get; set; }
        // protected bool Collapsed { get; set; } = true;    // hide by default
        [CascadingParameter]
        protected string CurrentUserId { get; set; }
        protected bool cssClassComment { get; set; } = true;    // hide by default
        protected string cssClassSave { get; set; } = "d-none";    // hide by default 
        protected bool IsDisabled { get; set; }

        protected CommentModel commentModel { get; set; }

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
                    commentModel.UserId = CurrentUserId;
                    commentModel.CommentId = CommentID;
                }
                else
                {
                    if (commentModel.UserId == CurrentUserId)
                    {
                        commentModel.CreatedAt = DateTime.Now;
                        commentModel.CommentText = MyText;

                        HttpResponseMessage respons = await CommentService.SaveComment(commentModel, WordId);
                    }
                }
                //if (!respons.IsSuccessStatusCode)
            }
        }

        protected async Task RemoveComment()
        {
            HttpResponseMessage respons = await CommentService.RemoveComment(CommentID);
        }

        protected async Task GetComment()
        {
            await CommentService.GetComment(CommentID);
        }
        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrEmpty(CommentID))
            {
                commentModel = new CommentModel();
                try
                {
                    commentModel = await CommentService.GetComment(CommentID);
                }
                catch { }

                if (commentModel.CommentId == null || commentModel.UserId == CurrentUserId)
                {
                    cssClassSave = null;
                    IsDisabled = false;
                }
                MyText = commentModel.CommentText;
            }
            //SetMyText();
            // CommentsCount = CommentsCount > 0 ? CommentsCount : null;
        }
    }
}

