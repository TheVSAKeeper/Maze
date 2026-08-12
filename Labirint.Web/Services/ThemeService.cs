using Blazored.LocalStorage;

namespace Labirint.Web.Services;

public sealed class ThemeService(IJSRuntime jsRuntime, ILocalStorageService localStorage)
{
    private const string StorageKey = "IsDarkMod";

    public bool IsDark { get; private set; } = true;

    public async Task InitializeAsync()
    {
        var isDark = await localStorage.GetItemAsync<bool?>(StorageKey);
        IsDark = isDark ?? true;
        await ApplyAsync();
    }

    public async Task ToggleAsync()
    {
        IsDark = IsDark == false;
        await localStorage.SetItemAsync(StorageKey, IsDark);
        await ApplyAsync();
    }

    private ValueTask ApplyAsync()
    {
        return jsRuntime.InvokeVoidAsync("labirintTheme.apply", IsDark ? "dark" : "light");
    }
}
