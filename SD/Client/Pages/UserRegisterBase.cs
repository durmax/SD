using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;
using SD.Client.Services;

namespace SD.Client.Pages
{
    public class UserRegisterBase : ComponentBase
    {
        public UserModel userModel = new UserModel();
        public Dictionary<string, string> foundUsers;

        //protected int? FriendsCount { set; get; }

        [Inject]
        public UserService UserService { set; get; }
        [Inject]
        public CurrentUserService CurrUsrService { set; get; }

        [Parameter]
        public string UserId { get; set; }
        //protected string Email { get; set; }

        protected string currUserId { get; set; }
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
        private async Task AddUserAsync(string currUserId, string email, string name)
        {
            UserModel userModel = new UserModel();
            userModel.UserId = currUserId;
            userModel.Email = email;
            userModel.Name = name;
            await UserService.AddUser(userModel);
        }

        protected async override Task OnInitializedAsync()
        {
            //if (!CurrUsrService.isAuthTested) await CurrUsrService.GetAuth();
            currUserId = CurrUsrService.id;
            if (currUserId != "0")
            {
                await AddUserAsync(currUserId, CurrUsrService.email, CurrUsrService.name);
            }

            if (string.IsNullOrWhiteSpace(UserId))
            {
                UserId = currUserId;
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
            }
        }
    }
}

