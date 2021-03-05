using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Client.Services;
using System.Linq;
using SD.Shared;

namespace SD.Client.Pages
{
    public class FriendsBase : ComponentBase
    {
        public IEnumerable<UserRelationshipsWithOneUser> foundUsers { set; get; }

        protected int? FriendsCount { set; get; }

        protected Dictionary<string, string> FriendsDictionary;

        [Inject]
        public UserService UserService { set; get; }

        [Inject]
        public RelationshipService RelationshipService { set; get; }

        [Inject]
        CurrentUserService CurrUsrService { set; get; }

        protected string currUserId { get; set; }

        protected bool sendFriendReqWait = false;

        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected string SearchDisplayClass { get; set; } = "d-none";

        public async Task SearchUser(string SearchText)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchDisplayClass = null;
                foundUsers = await UserService.SearchUser(currUserId, SearchText);
            }
            else
            {
                foundUsers = null;
                SearchDisplayClass = "d-none";
            }
        }

        protected async override Task OnInitializedAsync()
        {
                if (CurrUsrService.isAuthenticated)
                {
                    currUserId = CurrUsrService.id;

                    FriendsDictionary = new Dictionary<string, string>();
                    FriendsDictionary = await RelationshipService.GetAllFriends(currUserId);

                    FriendsCount = FriendsDictionary.Count();
                }
        }
    }
}

