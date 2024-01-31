using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;
using SD.Client.Services;
using System.Net.Http;
using System.Net.Http.Json;

namespace SD.Client.Pages
{
    public class UserRegisterBase : ComponentBase
    {
        public UserModel userModel = new();
        public Dictionary<string, string> foundUsers;

        //protected int? FriendsCount { set; get; }

        [Inject]
        public CurrentUserService CurrUsrService { set; get; }
        [Inject]
        public CurrentUser CurrentUser { set; get; }

        [Inject]
        HttpClient HttpClient { set; get; }

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
                            // userModel.UserId = UserId;
                            var status = await HttpClient.PostAsJsonAsync("api/User/Create", userModel);
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
                            var status = await HttpClient.PutAsJsonAsync($"api/User/UpdateUser/{userModel.UserId}", userModel);
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
            UserModel userModel = new()
            {
                UserId = currUserId,
                Email = email,
                Name = name
            };
            await HttpClient.PostAsJsonAsync("api/User/Create", userModel);
        }

        protected async override Task OnInitializedAsync()
        {
            //if (!CurrUsrService.isAuthTested) await CurrUsrService.GetAuth();
            CurrUserId = CurrentUser.id;
            if (CurrUserId != "0")
            {
                await AddUserAsync(CurrUserId, CurrentUser.email, CurrentUser.name);
            }

            if (string.IsNullOrWhiteSpace(UserId))
            {
                UserId = CurrUserId;
                CssDisplayNotCurrentUser = null;
            }

            if (!string.IsNullOrEmpty(UserId) && UserId != "0")
            {
                try
                {
                    userModel = await HttpClient.GetFromJsonAsync<UserModel>($"api/User/GetUserById/{UserId}");
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

