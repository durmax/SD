using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace sd.Client.Services;

public class LocalStorageAccessor : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageAccessor(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T> GetValueAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return default!;

        await WaitForReference();
        return await _module!.InvokeAsync<T>("get", key);
    }

    public async Task SetValueAsync<T>(string key, T value)
    {
        if (!string.IsNullOrWhiteSpace(key) && value != null)
        {
            await WaitForReference();
            await _module!.InvokeVoidAsync("set", key, value);
        }
    }

    public async Task Clear()
    {
        await WaitForReference();
        await _module!.InvokeVoidAsync("clear");
    }

    public async Task RemoveAsync(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            await WaitForReference();
            await _module!.InvokeVoidAsync("remove", key);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }

    private IJSObjectReference? _module;

    private async Task WaitForReference()
    {
        if (_module is null)
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/LocalStorageAccessor.js");
    }

   
}