using Microsoft.AspNetCore.Components;

namespace sd.Client.SharedUI.DictionaryLinks;

public partial class OtherPage
{
    [Parameter, EditorRequired] public string Href { get; set; } = default!;
    [Parameter, EditorRequired] public string Text { get; set; } = default!;
}
