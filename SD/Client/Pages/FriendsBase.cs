using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Client.Services;
using System.Linq;
using SD.Shared;
using System.Net.Http;
using System.Net.Http.Json;

namespace SD.Client.Pages
{
    public class FriendsBase : ComponentBase
    {
        public IEnumerable<UserRelationshipsWithOneUserDto> FoundUsers { set; get; }

        protected int? FriendsCount { set; get; }

        protected Dictionary<string, string> FriendsDictionary;

        [Inject]
        public RelationshipService RelationshipService { set; get; }

        [Inject]
        CurrentUser CurrentUser { set; get; }

        [Inject]
        HttpClient HttpClient { set; get; }

        protected string CurrUserId { get; set; }

        protected bool SendFriendReqWait = false;

        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected string SearchDisplayClass { get; set; } = "d-none";

        public async Task SearchUser(string SearchText)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchDisplayClass = null;
                CurrUserId ??= "0";
                FoundUsers = await HttpClient.GetFromJsonAsync<IEnumerable<UserRelationshipsWithOneUserDto>>($"api/User/GetUsersByTextNew/{CurrUserId}/{SearchText}");
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
                    CurrUserId = CurrentUser.id;

                    FriendsDictionary = new Dictionary<string, string>();
                    FriendsDictionary = await RelationshipService.GetAllFriends(CurrUserId);

                    FriendsCount = FriendsDictionary.Count;
                }
        }
    }
}

