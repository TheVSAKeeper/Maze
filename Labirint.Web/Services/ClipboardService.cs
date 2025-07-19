namespace Labirint.Web.Services;

public class ClipboardService(IJSRuntime jsRuntime)
{
    public ValueTask CopyToClipboard(string text)
    {
        return jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
}
