using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class OtherPageBase : ComponentBase
    {
        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Parameter]
        public OtherPageResModel otherPage { get; set; }
        
        [Parameter]
        public string FavSite { get; set; }

        [Parameter]
        public bool CanSetFavSite { get; set; }

        [Parameter]
        public string FLangCode { get; set; }

        [Parameter]
        public string TLangCode { get; set; }

        [Parameter] public EventCallback<DragEventArgs> OnDragStart { get; set; }


        public string Info { get; private set; }

        protected async Task Favorite(OtherPageResModel otherPage)
        {
            if (CanSetFavSite || string.IsNullOrWhiteSpace(FavSite))
            {
                await LocalStorageService.SetItemAsync("fav" + "-" + FLangCode + "-" + TLangCode, otherPage.Pattern);
                FavSite = await LocalStorageService.GetItemAsync<string>("fav" + "-" + FLangCode + "-" + TLangCode);
            }
            else
            {
                Info = "/Languages";
            }
        }

        protected async Task DragStart(DragEventArgs e)
        {
            Info = otherPage.Eval.ToString();
            e.Button = otherPage.Eval;
            
            await OnDragStart.InvokeAsync(e);
        }
    }
}