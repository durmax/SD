using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SD.Client.Services;
using sd.Shared;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class OtherPageBase : ComponentBase
    {
        [Inject]
        LocalStorageAccessor LocalStorageAccessor { get; set; }
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
                await LocalStorageAccessor.SetValueAsync($"fav-{FLangCode}{TLangCode}", otherPage.Pattern);
                FavSite = await LocalStorageAccessor.GetValueAsync<string>($"fav-{FLangCode}{TLangCode}");
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