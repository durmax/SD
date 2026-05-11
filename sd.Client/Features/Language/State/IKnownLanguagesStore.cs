using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace sd.Client.Features.Language.State;

public interface IKnownLanguagesStore
{
    Task<List<string>> GetAsync(CancellationToken ct = default);
    Task SaveAsync(IEnumerable<string> codes, CancellationToken ct = default);

    /// Adds code if missing (in-memory helper)
    bool EnsureContains(List<string> codes, string? code);

    /// Adds multiple codes if missing (in-memory helper)
    void EnsureContains(List<string> codes, params string?[] requiredCodes);
}
