using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Shared;
using SD.Client.Services;


namespace SD.Client.Pages
{
    public class UserRegisterBase : ComponentBase
    {
        public UserModel userModel = new UserModel();
        public IEnumerable<UserModel> users;

        [Inject]
        public UserService UserService { set; get; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected string UserId { get; set; }
        protected string Email { get; set; }

        protected List<string> FriendRequests { get; set; }
        protected List<string> Friends { get; set; }

        //[Parameter]
        //public List<string> KnownLangs { get; set; }
        //[Parameter]
        //public List<string> LearnLangs { get; set; }

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

        public async Task SearchUser(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                SearchDisplayClass = "";
                users = await UserService.SearchUser(text);
            }
            else
            {
                users = null;
                SearchDisplayClass = "d-none";
            }
        }

        public async Task SendFriendRequest(string ToUserId)
        {
            if (!string.IsNullOrWhiteSpace(ToUserId))
            {
                 await UserService.AddFriendRequest(UserId + "," + userModel.Name, ToUserId);
            }
        }

        public async Task RemoveUser()
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
                    UserId = user.FindFirst(c => c.Type == "oid")?.Value;
                    Email = user.FindFirst(c => c.Type == "email")?.Value;
                    userModel = await UserService.GetUserById(UserId);
                }
            }
            catch
            {
                //NavigationManager.NavigateTo("/");
            }

            if (userModel.UserId == null)
            {
                Registered = false;
                //userModel.UserId = UserId;
                //userModel.Email = Email;
                //userModel.Name = Email;

                await UserData();
            }
        }
    }
}

