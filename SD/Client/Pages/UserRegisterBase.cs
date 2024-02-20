using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;
using SD.Client.Services;
using System.Net.Http.Json;

namespace SD.Client.Pages
{
    public class UserRegisterBase : ComponentBase
    {
        public UserModel userModel = new();
        public Dictionary<string, string> foundUsers;

        [Inject]
        public CurrentUserService CurrentUser { set; get; }

        [Parameter]
        public string UserId { get; set; }
        //protected string Email { get; set; }

        protected string CurrUserId { get; set; }
        protected string CssDisplayNotCurrentUser = "d-none";

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
                            userModel.Role = Role.User;

                            var status = await CurrentUser.HttpClient.PostAsJsonAsync("api/User/Create", userModel);
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
                            var status = await CurrentUser.HttpClient.PutAsJsonAsync($"api/User/UpdateUser/{userModel.UserId}", userModel);
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

            if (!CurrentUser.IsAuthenticated)
            {
                CssDisplayNotCurrentUser = null;
            }
            else
            {
                try
                {
                    userModel = await CurrentUser.HttpClient.GetFromJsonAsync<UserModel>($"api/User/GetCurrentUser");
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

