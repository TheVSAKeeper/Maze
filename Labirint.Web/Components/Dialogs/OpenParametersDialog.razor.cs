using Blazored.LocalStorage;
using Labirint.Web.Common.Control.Schemes;
using Labirint.Web.Parameters;
using Labirint.Web.Services.Dialogs;
using Labirint.Web.Services.Toasts;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Components.Dialogs;

public partial class OpenParametersDialog
{
    private bool _isProcessing;
    private IControlScheme _controlScheme = null!;
    private LabyrinthParameters _parameters = new();

    [CascadingParameter]
    public required DialogInstance Instance { get; set; }

    [Inject]
    public required ToastService ToastService { get; set; }

    [Inject]
    public required ILocalStorageService LocalStorage { get; set; }

    [Inject]
    public required ControlSchemeService ControlSchemeService { get; set; }

    protected override void OnInitialized()
    {
        _parameters = GlobalParameters.Labyrinth.Clone();
        _controlScheme = ControlSchemeService.CurrentScheme;
    }

    private async Task UpdateAsync()
    {
        _isProcessing = true;

        try
        {
            await LocalStorage.SetItemAsync(LabyrinthParameters.LocalStorageKey, _parameters);
            GlobalParameters.Labyrinth = _parameters;

            if (ControlSchemeService.CurrentScheme != _controlScheme)
            {
                ControlSchemeService.CurrentScheme = _controlScheme;
            }

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
        _controlScheme = ControlSchemeService.DefaultScheme;
    }

    private void Cancel()
    {
        Instance.Cancel();
    }
}
