using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using sd.Client.Services;
using sd.Shared;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace sd.Client.Pages
{
    public class FriendshipBase : ComponentBase
    {
        [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { set; get; }
        [Inject] NavigationManager NavigationManager { set; get; }
        [Inject] protected CurrentUserService CurrentUser { set; get; }
        [Inject] protected ApiService ApiService { get; set; }
        [Parameter] public string FriendId { get; set; }
        [Parameter] public string FriendName { get; set; }
        [Parameter] public Relation Relationship { get; set; }
        protected bool waitBool = false;

        public async Task SendFriendRequest(string ToUserId)
        {
            waitBool = true;

            if (CurrentUser.IsAuthenticated && !string.IsNullOrWhiteSpace(ToUserId))
            {
                RelationshipModel relationship = new()
                {
                    RelationshipId = Guid.NewGuid().ToString(),
                    Relation = Relation.FriendRequestTo,
                    UserId1 = null, // Set in Server
                    UserId2 = ToUserId
                };
                var res = await ApiService.PostAsync<HttpResponseMessage>($"api/Relationship/AddRelationship", relationship);
                if (res.IsSuccessStatusCode)
                {
                    Relationship = Relation.FriendRequestTo;
                }
            }
            else
            {
                NavigationManager.NavigateTo("authentication/login");
            }
            waitBool = false;
        }

        public async Task RemoveFriend(string friendId)
        {
            waitBool = true;
            if (!string.IsNullOrWhiteSpace(friendId))
            {
                var res = await ApiService.DeleteAsync($"api/Relationship/RemoveFriendship/{Relation.Friend}/{friendId}");
                if (res.IsSuccessStatusCode)
                {
                    Relationship = Relation.None;
                }
            }
            waitBool = false;
        }

        public async Task AddFriend()
        {
            waitBool = true;

            if (!string.IsNullOrWhiteSpace(FriendId))
            {
                RelationshipModel relationship = new()
                {
                    RelationshipId = Guid.NewGuid().ToString(),
                    Relation = Relation.Friend,
                    UserId1 = null,  // Set in Server
                    UserId2 = FriendId
                };
                var res = await ApiService.PostAsync<HttpResponseMessage>($"api/Relationship/AddRelationship", relationship);

                if (res.IsSuccessStatusCode)
                {
                    await RemoveFriendRequest();
                    Relationship = Relation.Friend;
                }
            }
            waitBool = false;
        }
        public async Task RemoveFriendRequest()
        {
            waitBool = true;

            if (!string.IsNullOrWhiteSpace(FriendId))
            {
                var res = await ApiService.DeleteAsync($"api/Relationship/RemoveFriendship/{Relation.FriendRequestTo}/{FriendId}");
                if (res.IsSuccessStatusCode)
                {
                    Relationship = Relation.None;
                }
            }
            waitBool = false;
        }
    }
}
