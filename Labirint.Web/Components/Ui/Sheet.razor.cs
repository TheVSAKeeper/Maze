using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Labirint.Web.Components.Ui;

public partial class Sheet : IAsyncDisposable
{
    private ElementReference _sheet;
    private bool _isTrapped;

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    [EditorRequired]
    public required string Title { get; set; }

    [Parameter]
    public string CloseTitle { get; set; } = "Закрыть";

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    public async ValueTask DisposeAsync()
    {
        if (_isTrapped)
        {
            _isTrapped = false;
            await ReleaseAsync();
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender || _isTrapped == IsOpen)
        {
            return;
        }

        _isTrapped = IsOpen;

        await (IsOpen
            ? JSRuntime.InvokeVoidAsync("labirintDialog.trap", _sheet)
            : ReleaseAsync());
    }

    private async ValueTask ReleaseAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("labirintDialog.release", _sheet);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (TaskCanceledException)
        {
        }
    }

    private Task CloseAsync()
    {
        return IsOpen == false ? Task.CompletedTask : OnClose.InvokeAsync();
    }

    private Task OnKeyDown(KeyboardEventArgs args)
    {
        return args.Key == "Escape" ? CloseAsync() : Task.CompletedTask;
    }
}
