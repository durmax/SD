using Microsoft.AspNetCore.Components;
using SD.Client.Services;
using SD.Shared;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class WordBase : ComponentBase
    {
    protected bool Collapsed = true;    // hide by default

        [Inject]
        public WordService WordService { set; get; }

        [Parameter]
    public WordModel word { get; set; }

        protected string styleDeleted;
        protected string cssClassDelete ;

        protected async Task DeleteWord()
        {
          var response=  await WordService.RemoveWord(word.WordId);
            if ((int)response.StatusCode==200)
            {
                cssClassDelete = "d-none";
                styleDeleted = "text-decoration: line-through;";
            }
            //x = await response.Content.ReadAsStringAsync();
            
        }
    }
}
