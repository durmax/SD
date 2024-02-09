using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;
using System.Net.Http.Json;

namespace SD.Client.Pages
{
    public class FriendsBase : ComponentBase
    {
        public IEnumerable<UserRelationshipsWithOneUserDto> FoundUsers { set; get; }

        protected int? FriendsCount { set; get; }

        protected Dictionary<string, string> FriendsDictionary;

        [Inject]
        public CurrentUser CurrentUser { set; get; }

        protected bool SendFriendReqWait = false;

        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected string SearchDisplayClass { get; set; } = "d-none";

        public async Task SearchUser(string SearchText)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchDisplayClass = null;
                CurrentUser.id ??= "0";
                FoundUsers = await CurrentUser.httpClient.GetFromJsonAsync<IEnumerable<UserRelationshipsWithOneUserDto>>($"api/User/GetUsersByTextNew/{CurrentUser.id}/{SearchText}");
            }
            else
            {
                FoundUsers = null;
                SearchDisplayClass = "d-none";
            }
        }

        protected async override Task OnInitializedAsync()
        {
            if (CurrentUser.isAuthenticated)
            {
                FriendsDictionary = new Dictionary<string, string>();
                FriendsDictionary = await CurrentUser.httpClient.GetFromJsonAsync<Dictionary<string, string>>($"api/Relationship/GetAllFriends/{CurrentUser.id}");

                FriendsCount = FriendsDictionary.Count;
            }
        }
    }
}

