using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Shared;
using SD.Client.Services;
using System.Linq;

namespace SD.Client.Pages
{
    public class UserRegisterBase : ComponentBase
    {
        public UserModel userModel = new UserModel();
        public Dictionary<string, string> foundUsers;

        [Inject]
        public UserService UserService { set; get; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Parameter]
        public string UserId { get; set; }
        protected string Email { get; set; }

        protected string CurrentUserId { get; set; }
        protected string cssDisplayNotCurrentUser = "d-none";

        protected List<string> FriendRequests { get; set; }
        protected List<string> Friends { get; set; }
        protected bool sendFriendReqWait = false;

        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected bool Registered { get; set; } = true;
        protected string SearchDisplayClass { get; set; } = "d-none";

        public async Task UserData()
        {
            if (userModel.Email != null)
            {
                try
                {
                    if (!Registered)
                    {
                        // userModel.UserId = UserId;
                        var status = await UserService.AddUser(userModel);
                        if (status.IsSuccessStatusCode)
                        {
                            Info = $"Willcome {userModel.Email}!, your data saved successfully";
                            Registered = true;
                        }
                        else
                        {
                            Info = status.ReasonPhrase;
                        }
                    }
                    else
                    {
                        // userModel.Email = Email;
                        var status = await UserService.UpdateUser(userModel.UserId, userModel);
                        Info = status.ReasonPhrase;
                    }
                }
                catch (Exception ex)
                {
                    Info = "Check if your data saved successfully please! ";
                    // Error
                    Info += ex.Message;
                }
                InfoDisplayClass = "";
            }
        }

        public async Task SearchUser(string SearchText)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchDisplayClass = "";
                foundUsers = await UserService.SearchUser(CurrentUserId + "," + userModel.Name, SearchText);
            }
            else
            {
                foundUsers = null;
                SearchDisplayClass = "d-none";
            }
        }

        public async Task SendFriendRequest(string ToUserId, string friendName)
        {
            sendFriendReqWait = true;

            if (!string.IsNullOrWhiteSpace(UserId) && !string.IsNullOrWhiteSpace(ToUserId))
            {
                var res = await UserService.AddFriendRequest(UserId + "," + userModel.Name, ToUserId);

                if (res.IsSuccessStatusCode)
                {
                    Info = $"The friend request sent to {friendName} successfully";
                    InfoDisplayClass = null;
                }
            }
            else
            {
                Info = $"Please, Login to add Friends";
                InfoDisplayClass = null;
            }
            sendFriendReqWait = false;
        }

        public async Task RemoveFriend(string friendId)
        {
            sendFriendReqWait = true;
            if (!string.IsNullOrWhiteSpace(friendId))
            {
               await UserService.RemoveFriend(UserId, friendId);
            }
           sendFriendReqWait = false;
        }

            public async Task RemoveUser() // remove current user
        {
            try
            {
                var res = await UserService.RemoveUser(UserId);
                Info = $"{userModel.Email} deleted successfully";
            }
            catch (Exception ex)
            {
                Info = "Check if your data deleted successfully please! ";
                // Error
                Info += ex.Message;
            }
            InfoDisplayClass = "";
        }
        protected async override Task OnInitializedAsync()
        {
            try
            {
                var user = (await authenticationStateTask).User;

                if (user.Identity.IsAuthenticated)
                {
                    CurrentUserId = user.FindFirst(c => c.Type == "oid")?.Value;
                    Email = user.FindFirst(c => c.Type == "email")?.Value;
                }
            }
            catch
            {
                //NavigationManager.NavigateTo("/");
            }

            if (string.IsNullOrWhiteSpace(UserId))
            {
                UserId = CurrentUserId;
                cssDisplayNotCurrentUser = null;
            }

            if (!string.IsNullOrEmpty(UserId))
            {
                try
                {
                    userModel = await UserService.GetUserById(UserId);
                }
                catch
                {
                    Registered = false;
                    await UserData();
                }
            }
        }
    }
}

