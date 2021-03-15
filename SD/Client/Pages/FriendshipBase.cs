using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class FriendshipBase : ComponentBase
    {
        protected bool waitBool = false;
        [Parameter]
        public string UserId { get; set; }
        [Parameter]
        public string friendId { get; set; }
        [Parameter]
        public string friendName { get; set; }

        [Parameter]
        public Relation Relationship { get; set; }
        [Inject]
        public RelationshipService RelationshipService { set; get; }
        [Inject]
        NavigationManager NavigationManager { set; get; }

        public async Task SendFriendRequest(string ToUserId)
        {
            waitBool = true;

            if (UserId != "0" && !string.IsNullOrWhiteSpace(ToUserId))
            {
                RelationshipModel relationship = new RelationshipModel()
                {
                    RelationshipId = Guid.NewGuid().ToString(),
                    Reletion = Relation.FriendRequestTo,
                    UserId1 = UserId,
                    UserId2 = ToUserId
                };
                var res = await RelationshipService.AddRelationship(relationship);
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
                var res = await RelationshipService.RemoveFriendship(UserId,Relation.Friend, friendId);
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

            if (!string.IsNullOrWhiteSpace(friendId))
            {
                RelationshipModel relationship = new RelationshipModel()
                {
                    RelationshipId = Guid.NewGuid().ToString(),
                    Reletion = Relation.Friend,
                    UserId1 = UserId,
                    UserId2 = friendId
                };
                var res = await RelationshipService.AddRelationship(relationship);

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

            if (!string.IsNullOrWhiteSpace(friendId))
            {
                var res = await RelationshipService.RemoveFriendship(UserId,Relation.FriendRequestTo, friendId);
                if (res.IsSuccessStatusCode)
                {
                    Relationship = Relation.None;
                }
            }
            waitBool = false;
        }
    }
}
