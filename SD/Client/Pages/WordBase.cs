using Microsoft.AspNetCore.Components;
using SD.Shared;

namespace SD.Client.Pages
{
    public class WordBase : ComponentBase
    {
    protected bool Collapsed = true;    // hide by default

    [Parameter]
    public WordModel word { get; set; }
    }
}
