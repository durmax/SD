using Microsoft.AspNetCore.Components;
using SD.Shared;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class FriendshipBase : ComponentBase
    {
        protected bool waitBool = false;
        [Parameter]
        public string UserId { get; set; }
        [Parameter]
        public string FriendId { get; set; }
        [Parameter]
        public string FriendName { get; set; }

        [Parameter]
        public Relation Relationship { get; set; }

        [Inject]
        NavigationManager NavigationManager { set; get; }

        [Inject]
        protected CurrentUser CurrentUser { set; get; }

        public async Task SendFriendRequest(string ToUserId)
        {
            waitBool = true;

            if (UserId != "0" && !string.IsNullOrWhiteSpace(ToUserId))
            {
                RelationshipModel relationship = new()
                {
                    RelationshipId = Guid.NewGuid().ToString(),
                    Reletion = Relation.FriendRequestTo,
                    UserId1 = UserId,
                    UserId2 = ToUserId
                };
                var res = await  CurrentUser.httpClient.PostAsJsonAsync($"api/Relationship/AddRelationship", relationship);
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
                var res = await CurrentUser.httpClient.DeleteAsync($"api/Relationship/RemoveFriendship/{UserId}/{Relation.Friend}/{friendId}");
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
                    Reletion = Relation.Friend,
                    UserId1 = UserId,
                    UserId2 = FriendId
                };
                var res = await CurrentUser.httpClient.PostAsJsonAsync($"api/Relationship/AddRelationship", relationship);

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
                var res = await CurrentUser.httpClient.DeleteAsync($"api/Relationship/RemoveFriendship/{UserId}/{Relation.FriendRequestTo}/{FriendId}");
                if (res.IsSuccessStatusCode)
                {
                    Relationship = Relation.None;
                }
            }
            waitBool = false;
        }
    }
}
