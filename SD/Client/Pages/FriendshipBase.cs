using Microsoft.AspNetCore.Components;
using SD.Client.Services;
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

        [Inject]
        public UserService UserService { set; get; }
        [Inject]
        NavigationManager NavigationManager { set; get; }

        public async Task SendFriendRequest(string ToUserId)
        {
            waitBool = true;

            if (UserId != "0" && !string.IsNullOrWhiteSpace(ToUserId))
            {
                var res = await UserService.AddFriendRequest(UserId, ToUserId);
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
                var res = await UserService.RemoveFriend(UserId, friendId);
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
                var res = await UserService.AddFriend(UserId, friendId);

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
                var res = await UserService.RemoveFriendRequest(UserId, friendId);
                if (res.IsSuccessStatusCode)
                {
                    friendshipState = null;
                }
            }
            waitBool = false;
        }
    }
}
