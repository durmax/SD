namespace sd.Client.Contracts;

public readonly record struct LanguageOption(string Code, string Name);

public readonly record struct LanguagePair(LanguageOption From, LanguageOption To);
