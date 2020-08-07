using Microsoft.AspNetCore.Components;
using MongoDB.Bson;
using SD.Client.Services;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Security;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class LikeBase : ComponentBase
    {
        [Inject]
        public WordService WordService { set; get; }
        [Inject]
        public UserService UserService { set; get; }
        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string CurrentUserId { get; set; }

        [Parameter]
        public bool CULiked { get; set; }

        protected string CULikeClass;

        [Parameter]
        public string WordId { get; set; }

        [Parameter]
        public int? LikesCount { get; set; }

        protected bool Collapsed { get; set; } = true;    // hide by default

        protected Dictionary<string, Tuple<string, string>> likedUsers;

        protected async Task Like()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUserId) && CurrentUserId != "0")
            {
                LikesCount = await WordService.Like(CurrentUserId, WordId);
                CULiked = !CULiked;
                CULikeClass = CULiked ? "text-primary" : null;
            }
            else
            {
                NavigationManager.NavigateTo("authentication/login");
            }
        }

        protected async Task GetLikedUsers(int? likesCount)
        {
            if (!Collapsed && likesCount!=null)
            {
                likedUsers = await WordService.GetLikedUsers(CurrentUserId, WordId);
            }
        }

        protected override void OnInitialized()
        {
            //CULikeClass = CULiked ? "border border-primary" : null;
            LikesCount = LikesCount > 0 ? LikesCount : null;
        }
    }
}
