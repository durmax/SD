using sd.Client.Helpers;
using sd.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace sd.Client.Features.Language.State;
public sealed class KnownLanguagesStore : IKnownLanguagesStore
{
    private readonly LocalStorageAccessor _localStorage;
    private readonly KnownLangsService _knownLangsService;

    public KnownLanguagesStore(LocalStorageAccessor localStorage, KnownLangsService knownLangsService)
    {
        _localStorage = localStorage;
        _knownLangsService = knownLangsService;
    }

    public async Task<List<string>> GetAsync(CancellationToken ct = default)
    {
        var langsStr = await _localStorage.GetValueAsync<string>(LangStorageKeys.KnownLangs);
        _knownLangsService.LangsStr = langsStr;

        return ParseLangsStr(langsStr);
    }

    public async Task SaveAsync(IEnumerable<string> codes, CancellationToken ct = default)
    {
        var normalized = Normalize(codes);

        // Store as ",en,de,ar"
        var langsStr = string.Join("", normalized.Select(x => "," + x));

        _knownLangsService.LangsStr = langsStr;
        await _localStorage.SetValueAsync(LangStorageKeys.KnownLangs, langsStr);
    }

    public bool EnsureContains(List<string> codes, string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;

        if (!codes.Contains(code))
        {
            codes.Add(code);
            return true;
        }

        return false;
    }

    public void EnsureContains(List<string> codes, params string?[] requiredCodes)
    {
        foreach (var code in requiredCodes)
            EnsureContains(codes, code);
    }

    private static List<string> Normalize(IEnumerable<string> codes) =>
        (codes ?? Enumerable.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static List<string> ParseLangsStr(string? langsStr)
    {
        if (string.IsNullOrWhiteSpace(langsStr) || langsStr == "null")
            return new List<string>();

        return langsStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
