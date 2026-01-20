using sd.Client.Features.Language.Domain;
using sd.Shared;
using System.Collections.Generic;

namespace sd.Client.Features.Language.State;

public sealed class LanguageState
{
    public LearningPair Pair { get; set; } = new("en", "de");

    // Stored as culture code like "de-DE"
    public string? UiCultureCode { get; set; }

    // Stored as CSV in local storage today, so we keep it compatible
    public string KnownLangsCsv { get; set; } = "";

    public List<DictionaryProviderDto> DictionaryProviders { get; set; } = new();
}
