using Labirint.Web.Services.Dialogs;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Components.Dialogs;

public partial class WinDialog
{
    [Parameter]
    public required Func<Task> OnRestart { get; set; }

    [Parameter]
    public required RandomGenerator Seeder { get; set; }

    [CascadingParameter]
    private DialogInstance Instance { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private void Close()
    {
        Instance.Close(true);
    }

    private void RepeatGame()
    {
        NavigationManager.NavigateTo(Seeder.Link);
        Instance.Close(false);
    }

    private async Task RestartGameAsync()
    {
        Seeder.Reload(true);
        await OnRestart();

        Instance.Close(false);
    }
}
