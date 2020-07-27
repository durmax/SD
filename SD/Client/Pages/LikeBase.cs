using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class LikeBase : ComponentBase
    {
        [Inject]
        public WordService WordService { set; get; }
        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string CurrentUserId { get; set; }

        [Parameter]
        public bool CULiked { get; set; }

        protected string CULikeClass;

        [Parameter]
        public string WordId { get; set; }

        [Parameter]
        public int? LikesCount { get; set; }

        protected async Task Like()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUserId) && CurrentUserId != "0")
            {
                LikesCount = await WordService.Like(CurrentUserId, WordId);
                CULiked = !CULiked;
                CULikeClass = CULiked ? "text-primary" : null;
            }
            else
            {
                NavigationManager.NavigateTo("authentication/login");
            }
        }

        protected override void OnInitialized()
        {
            CULikeClass = CULiked ? "text-primary" : null;
        }
    }
}
