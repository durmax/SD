using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace sd.Client.Services;

public class LocalStorageAccessor : IAsyncDisposable
{
    private Lazy<IJSObjectReference> _accessorJsRef = new();
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageAccessor(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T> GetValueAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return default(T);

        await WaitForReference();
        var result = await _accessorJsRef.Value.InvokeAsync<T>("get", key);

        return result;
    }

    public async Task SetValueAsync<T>(string key, T value)
    {
        if (!string.IsNullOrWhiteSpace(key) && value != null)
        {
            await WaitForReference();
            await _accessorJsRef.Value.InvokeVoidAsync("set", key, value);
        }
    }

    public async Task Clear()
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("clear");
    }

    public async Task RemoveAsync(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            await WaitForReference();
            await _accessorJsRef.Value.InvokeVoidAsync("remove", key);
        }
    }

    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/LocalStorageAccessor.js"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }
}