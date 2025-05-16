using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using sd.Shared;
using System.Net.Http.Json;
using SD.Client.Services;

namespace SD.Client.Pages
{
    public class FriendsBase : ComponentBase
    {
        public IEnumerable<UserRelationshipsWithOneUserDto> FoundUsers { set; get; }

        protected int? FriendsCount { set; get; }

        protected Dictionary<string, string> FriendsDictionary;

        [Inject]
        public CurrentUserService CurrentUser { set; get; }
        [Inject]
        protected ApiService ApiService { get; set; }

        protected bool SendFriendReqWait = false;

        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected string SearchDisplayClass { get; set; } = "d-none";

        public async Task SearchUser(string SearchText)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchDisplayClass = null;

                FoundUsers = await ApiService.GetAsync<IEnumerable<UserRelationshipsWithOneUserDto>>($"api/User/GetUsersByText/{SearchText}");
            }
            else
            {
                FoundUsers = null;
                SearchDisplayClass = "d-none";
            }
        }

        protected async override Task OnInitializedAsync()
        {
            if (CurrentUser.IsAuthenticated)
            {
                FriendsDictionary = new Dictionary<string, string>();
                FriendsDictionary = await ApiService.GetAsync<Dictionary<string, string>>($"api/Relationship/GetAllFriends");

                FriendsCount = FriendsDictionary.Count;
            }
        }
    }
}

