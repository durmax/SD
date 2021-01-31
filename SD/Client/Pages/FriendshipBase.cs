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
        public string friendshipState { get; set; }

        //[Inject]
        //public UserService UserService { set; get; }
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
                    Reletion = Reletion.FriendRequest,
                    UserId1 = UserId,
                    UserId2 = ToUserId
                };
                var res = await RelationshipService.AddRelationship(relationship);
                if (res.IsSuccessStatusCode)
                {
                    friendshipState = "CrrRequest";
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
                var res = await RelationshipService.RemoveFriendship(UserId,Reletion.Friend, friendId);
                if (res.IsSuccessStatusCode)
                {
                    friendshipState = null;
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
                    Reletion = Reletion.Friend,
                    UserId1 = UserId,
                    UserId2 = friendId
                };
                var res = await RelationshipService.AddRelationship(relationship);

                if (res.IsSuccessStatusCode)
                {
                    await RemoveFriendRequest();
                    friendshipState = "Friends";
                }
            }
            waitBool = false;
        }
        public async Task RemoveFriendRequest()
        {
            waitBool = true;

            if (!string.IsNullOrWhiteSpace(friendId))
            {
                var res = await RelationshipService.RemoveFriendship(UserId,Reletion.FriendRequest, friendId);
                if (res.IsSuccessStatusCode)
                {
                    friendshipState = null;
                }
            }
            waitBool = false;
        }
    }
}
