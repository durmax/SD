using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class MyWordsBase : ComponentBase
    {
        [Inject]
        public WordService WordService { set; get; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected string UserId { get; set; }

    }
}
