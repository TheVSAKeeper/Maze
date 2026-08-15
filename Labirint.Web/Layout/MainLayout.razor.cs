using Blazored.LocalStorage;
using Labirint.Web.Common.Ui;
using Labirint.Web.Components.Dialogs;
using Labirint.Web.Parameters;
using Labirint.Web.Services;
using Labirint.Web.Services.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

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

    [Inject]
    public required ILogger<MainLayout> Logger { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await ThemeService.InitializeAsync();
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Не удалось применить тему, остаётся тема по умолчанию");
        }

        GlobalParameters.Labyrinth = await LoadParametersAsync() ?? new LabyrinthParameters();
    }

    private async Task<LabyrinthParameters?> LoadParametersAsync()
    {
        try
        {
            LabyrinthParameters? labyrinthParameters = await LocalStorage.GetItemAsync<LabyrinthParameters>(LabyrinthParameters.LocalStorageKey);

            // TODO: разовая миграция без версии настроек – заводить версионирование, когда сменится следующий дефолт
            if (labyrinthParameters?.Color == LabyrinthParameters.LegacyDefaultColor)
            {
                labyrinthParameters.Color = LabyrinthParameters.DefaultColor;
                await LocalStorage.SetItemAsync(LabyrinthParameters.LocalStorageKey, labyrinthParameters);
            }

            return labyrinthParameters;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Не удалось прочитать параметры лабиринта, остаются значения по умолчанию");
            return null;
        }
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
        return DialogService.ShowAsync<OpenParametersDialog>("Настройки", options: new DialogOptions
        {
            CloseOnBackdropClick = false,
            Width = DialogWidth.Small,
        });
    }
}
