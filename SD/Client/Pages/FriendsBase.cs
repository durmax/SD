using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using System.Linq;

namespace SD.Client.Pages
{
    public class FriendsBase : ComponentBase
    {
        public Dictionary<string, Tuple<string, string>> foundUsers;

        protected int? FriendsCount { set; get; }

        protected Dictionary<string, string> FriendsDictionary;

        [Inject]
        public UserService UserService { set; get; }

        [Inject]
        public RelationshipService RelationshipService { set; get; }

        [Inject]
        CurrentUserService CurrUsrService { set; get; }

        protected string CurrentUserId { get; set; }

        protected bool sendFriendReqWait = false;

        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected string SearchDisplayClass { get; set; } = "d-none";

        public async Task SearchUser(string SearchText)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchDisplayClass = null;
                foundUsers = await UserService.SearchUser(CurrentUserId, SearchText);
            }
            else
            {
                foundUsers = null;
                SearchDisplayClass = "d-none";
            }
        }

        protected async override Task OnInitializedAsync()
        {

            if (await CurrUsrService.IsAuth())
            {
                CurrentUserId = await CurrUsrService.GetCurrUsrId();
                FriendsDictionary = new Dictionary<string, string>();
                FriendsDictionary = await RelationshipService.GetAllFriends(CurrentUserId);

                FriendsCount = FriendsDictionary.Count();
            }

        }
    }
}

