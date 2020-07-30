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
        NavigationManager NavigationManager { set; get; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected string CurrentUserId { get; set; }

        protected bool sendFriendReqWait = false;

        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected string SearchDisplayClass { get; set; } = "d-none";

        public async Task SearchUser(string SearchText)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchDisplayClass = "";
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
            try
            {
                var user = (await authenticationStateTask).User;

                if (user.Identity.IsAuthenticated)
                {
                    CurrentUserId = user.FindFirst(c => c.Type == "oid")?.Value;
                    //Email = user.FindFirst(c => c.Type == "email")?.Value;

                    FriendsDictionary = new Dictionary<string, string>();
                    FriendsDictionary = await UserService.GetAllFriends(CurrentUserId);

                    FriendsCount = FriendsDictionary.Count();
                }
                else
                {
                    CurrentUserId = "0";
                }
            }
            catch
            {
                NavigationManager.NavigateTo("/");
            }
        }
    }
}

