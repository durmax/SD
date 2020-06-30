using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class FriendRequestBase : ComponentBase
    {
        [Inject]
        public UserService UserService { set; get; }
        
        [Parameter]
        public string UserId { get; set; }
        [Parameter]
        public string friendId { get; set; }
        [Parameter]
        public string friendName { get; set; }
        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected bool FriendWait = false;

        //protected bool FriendAdded = false;
        //protected bool FriendReqRemoved = false;

        protected string FriendReqState = null;

        // protected List<string> FriendRequestsNames;
        // protected Dictionary<string, string> FriendRequestsDictionary;

        public async Task AddFriend()
        {   
            FriendWait = true;
          
            if (!string.IsNullOrWhiteSpace(friendId))
            {
                await UserService.AddFriend(UserId, friendId);
                await RemoveFriendRequest();

               var user1= await UserService.GetUserById(friendId);
                Info = $"You added {user1.Name} to your friends successfully";
                InfoDisplayClass = null;
            }
            //FriendAdded = true;
            FriendReqState = "Added";
            FriendWait = false;
        }
        public async Task RemoveFriendRequest()
        {
            FriendWait = true;

            if (!string.IsNullOrWhiteSpace(friendId))
            {
                await UserService.RemoveFriendRequest(UserId, friendId);

                var user1 = await UserService.GetUserById(friendId);
                Info = $"{user1.Name} friend request removed successfully";
                InfoDisplayClass = null;
            }
            //FriendReqRemoved = true;
            FriendReqState = "Removed";
            FriendWait = false;
        }
    }
}
