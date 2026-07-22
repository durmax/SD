using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using sd.Client.Services;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace sd.Client.Pages
{
    public class UserRegisterBase : ComponentBase
    {
        public UserModel userModel = new();
        public Dictionary<string, string> foundUsers;

        [Inject] public ILogger<UserRegisterBase> Log { get; set; }
        [Inject] public CurrentUserService CurrentUser { set; get; }
        [Inject] protected ApiService ApiService { get; set; }

        protected string CurrUserId { get; set; }
        protected string Info { get; set; }
        protected string InfoDisplayClass { get; set; } = "d-none";
        protected bool Registered { get; set; } = true;
        protected string CssDisplayNotCurrentUser = "d-none";

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

                            var status = await ApiService.PostAsync<HttpResponseMessage>("api/User/Create", userModel);
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
                            var status = await ApiService.PutAsync<HttpResponseMessage>($"api/User/UpdateUser/{userModel.UserId}", userModel);
                            Info = status.ReasonPhrase;
                        }
                    }
                    catch (Exception ex)
                    {
                        Info = "Check if your data saved successfully please! ";
                        // Error
                        Info += ex.Message;

                        Log.LogError(ex.Message);
                    }
                    Log.LogInformation(Info);
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
                    userModel = await ApiService.GetAsync<UserModel>($"api/User/GetCurrentUser");
                }
                catch
                {
                    Registered = false;
                    await SaveUserData();
                    Log.LogError("Error in getting current user data");
                }
            }
        }
    }
}

