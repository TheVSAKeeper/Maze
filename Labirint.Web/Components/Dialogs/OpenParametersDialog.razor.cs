using Blazored.LocalStorage;
using Labirint.Web.Parameters;
using Labirint.Web.Services.Dialogs;
using Labirint.Web.Services.Toasts;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Components.Dialogs;

public partial class OpenParametersDialog
{
    private bool _isProcessing;
    private ControlSchemeSwitcher _controlScheme = null!;
    private LabyrinthParameters _parameters = new();

    [CascadingParameter]
    public required DialogInstance Instance { get; set; }

    [Inject]
    public required ToastService ToastService { get; set; }

    [Inject]
    public required ILocalStorageService LocalStorage { get; set; }

    protected override void OnInitialized()
    {
        _parameters = GlobalParameters.Labyrinth.Clone();
    }

    private async Task UpdateAsync()
    {
        _isProcessing = true;

        try
        {
            await LocalStorage.SetItemAsync(LabyrinthParameters.LocalStorageKey, _parameters);
            GlobalParameters.Labyrinth = _parameters;
            ToastService.Show("Сохранено!", UiSeverity.Success);
            Instance.Close();
        }
        catch
        {
            ToastService.Show("Не удалось сохранить", UiSeverity.Error);
        }

        _isProcessing = false;
    }

    private void Reset()
    {
        _parameters = new();
        _controlScheme.ControlSchemeService.Reset();
    }

    private void Cancel()
    {
        Instance.Cancel();
    }
}
