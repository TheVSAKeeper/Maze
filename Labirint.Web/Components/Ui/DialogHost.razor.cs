using Labirint.Web.Common.Ui;
using Labirint.Web.Services.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Labirint.Web.Components.Ui;

public partial class DialogHost : IDisposable
{
    private ElementReference _backdrop;
    private int _shownCount;

    [Inject]
    public required DialogService DialogService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    public void Dispose()
    {
        DialogService.Changed -= OnChanged;
    }

    protected override void OnInitialized()
    {
        DialogService.Changed += OnChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        int count = DialogService.Instances.Count;

        if (count > 0 && count != _shownCount)
        {
            await JSRuntime.InvokeVoidAsync("labirintDialog.trap", _backdrop);
        }
        else if (count == 0 && _shownCount > 0)
        {
            await JSRuntime.InvokeVoidAsync("labirintDialog.release");
        }

        _shownCount = count;
    }

    private static string GetWidthClass(DialogInstance instance)
    {
        return instance.Options.Width switch
        {
            DialogWidth.Small => "ui-dialog-panel--small",
            DialogWidth.Large => "ui-dialog-panel--large",
            DialogWidth.Medium => "ui-dialog-panel--medium",
            _ => "ui-dialog-panel--auto",
        };
    }

    private void OnChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnBackdropClick()
    {
        DialogInstance? topmost = GetTopmost();

        if (topmost?.Options.CloseOnBackdropClick == true)
        {
            topmost.Cancel();
        }
    }

    private void OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key != "Escape")
        {
            return;
        }

        DialogInstance? topmost = GetTopmost();

        if (topmost?.Options.CloseOnEscape == true)
        {
            topmost.Cancel();
        }
    }

    private DialogInstance? GetTopmost()
    {
        return DialogService.Instances.Count > 0
            ? DialogService.Instances[^1]
            : null;
    }
}
