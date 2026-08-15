using Labirint.Web.Common.Ui;
using Labirint.Web.Services.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Labirint.Web.Components.Ui;

public partial class DialogHost : IDisposable
{
    private readonly Dictionary<Guid, ElementReference> _panels = [];
    private readonly List<TrappedPanel> _trapped = [];

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
        List<Guid> shown = DialogService.Instances.Select(instance => instance.Id).ToList();

        for (int index = _trapped.Count - 1; index >= 0; index--)
        {
            TrappedPanel trapped = _trapped[index];

            if (shown.Contains(trapped.Id))
            {
                continue;
            }

            _trapped.RemoveAt(index);
            _panels.Remove(trapped.Id);

            await ReleaseAsync(trapped.Panel);
        }

        foreach (Guid id in shown)
        {
            if (_trapped.Exists(trapped => trapped.Id == id) || _panels.TryGetValue(id, out ElementReference panel) == false)
            {
                continue;
            }

            _trapped.Add(new(id, panel));
            await JSRuntime.InvokeVoidAsync("labirintDialog.trap", panel);
        }
    }

    private async ValueTask ReleaseAsync(ElementReference panel)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("labirintDialog.release", panel);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (TaskCanceledException)
        {
        }
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

    private sealed record TrappedPanel(Guid Id, ElementReference Panel);
}
