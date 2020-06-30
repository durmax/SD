using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
           string friendIdAndName = friendId + ","+ friendName;
           
            if (!string.IsNullOrWhiteSpace(friendIdAndName))
            {
                await UserService.AddFriend(UserId, friendIdAndName);
                await RemoveFriendRequest();
                //FriendRequestsDictionary.Remove(friendIdAndName);
                string[] friendName = friendIdAndName.Split(",");
                Info = $"You added {friendName[1]} to your friends successfully";
                InfoDisplayClass = null;
            }
            //FriendAdded = true;
            FriendReqState = "Added";
            FriendWait = false;
        }
        public async Task RemoveFriendRequest()
        {
            FriendWait = true;
            string friendIdAndName = friendId + "," + friendName;

            if (!string.IsNullOrWhiteSpace(friendIdAndName))
            {
                await UserService.RemoveFriendRequest(UserId, friendIdAndName);
               // FriendRequestsDictionary.Remove(friendIdAndName);
                string[] friendName = friendIdAndName.Split(",");
                Info = $"{friendName[1]} friend request removed successfully";
                InfoDisplayClass = null;
            }
            //FriendReqRemoved = true;
            FriendReqState = "Removed";
            FriendWait = false;
        }
    }
}
