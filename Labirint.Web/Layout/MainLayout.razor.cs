using Blazored.LocalStorage;
using Labirint.Web.Common.Ui;
using Labirint.Web.Components.Dialogs;
using Labirint.Web.Parameters;
using Labirint.Web.Services;
using Labirint.Web.Services.Dialogs;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Layout;

public partial class MainLayout
{
    [Inject]
    public required ILocalStorageService LocalStorage { get; set; }

    [Inject]
    public required ThemeService ThemeService { get; set; }

    [Inject]
    public required DialogService DialogService { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await ThemeService.InitializeAsync();

        LabyrinthParameters? labyrinthParameters = await LocalStorage.GetItemAsync<LabyrinthParameters>(LabyrinthParameters.LocalStorageKey);
        GlobalParameters.Labyrinth = labyrinthParameters ?? new LabyrinthParameters();
    }

    private void Reload()
    {
        NavigationManager.Refresh(true);
    }

    private Task ToggleThemeAsync()
    {
        return ThemeService.ToggleAsync();
    }

    private Task OpenParametersAsync()
    {
        return DialogService.ShowAsync<OpenParametersDialog>("Параметры", options: new DialogOptions
        {
            CloseOnBackdropClick = false,
            Width = DialogWidth.Small,
        });
    }
}
