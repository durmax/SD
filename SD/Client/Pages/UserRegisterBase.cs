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
        public Dictionary<string, string> foundUsers;

        protected int? FriendsCount { set; get; }

        [Inject]
        public UserService UserService { set; get; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Parameter]
        public string UserId { get; set; }
        protected string Email { get; set; }

        protected string CurrentUserId { get; set; }
        protected string cssDisplayNotCurrentUser = "d-none";

        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected bool Registered { get; set; } = true;

        public async Task SaveUserData()
        {
            if (userModel != null)
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
                else
                {
                    CurrentUserId = "0";
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

            if (!string.IsNullOrEmpty(UserId) && UserId != "0")
            {
                try
                {
                    userModel = await UserService.GetUserById(UserId);
                }
                catch
                {
                    Registered = false;
                    await SaveUserData();
                }

                FriendsCount = userModel?.Friends.Count ?? 0;
            }
        }
    }
}

