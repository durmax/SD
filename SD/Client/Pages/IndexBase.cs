using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using System.Threading.Tasks;
using SD.Shared;

namespace SD.Client.Pages
{
    public class IndexBase : ComponentBase
    {
        public WordModel wordModel = new WordModel();

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Inject]
        public UserService UserService { set; get; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected string UserId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            wordModel.WordLang = await LocalStorageService.GetItemAsync<string>("FLang");
            wordModel.ToLang = await LocalStorageService.GetItemAsync<string>("TLang");

            if (string.IsNullOrWhiteSpace(wordModel.WordLang) || wordModel.WordLang == "null" || string.IsNullOrWhiteSpace(wordModel.ToLang) || wordModel.ToLang == "null")
            {
                NavigationManager.NavigateTo("Languages");
            }

            try
            {
                var user = (await authenticationStateTask).User;

                if (user.Identity.IsAuthenticated)
                {
                    UserId = user.FindFirst(c => c.Type == "oid")?.Value;
                    UserModel userModel = new UserModel()
                    {
                        UserId = UserId,
                        Email = user.FindFirst(c => c.Type == "email")?.Value,
                        Name = user.Identity.Name//user.FindFirst(c => c.Type == ClaimTypes.Surname)?.Value
                    };
                    await UserService.AddUser(userModel);
                }
            }
            catch
            {
                //NavigationManager.NavigateTo("/");
            }
        }

    }
}
