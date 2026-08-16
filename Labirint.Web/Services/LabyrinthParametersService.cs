using Labirint.Web.Parameters;
using Microsoft.Extensions.Logging;

namespace Labirint.Web.Services;

public sealed class LabyrinthParametersService(LocalStorageService localStorage, ILogger<LabyrinthParametersService> logger)
{
    private LabyrinthParameters _current = new();

    public event EventHandler? Changed;

    public LabyrinthParameters Current
    {
        get => _current;
        private set
        {
            _current = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task InitializeAsync()
    {
        Current = await LoadAsync() ?? new LabyrinthParameters();
    }

    public async Task SaveAsync(LabyrinthParameters parameters)
    {
        await localStorage.SetItemAsync(LabyrinthParameters.LocalStorageKey, parameters);
        Current = parameters;
    }

    private async Task<LabyrinthParameters?> LoadAsync()
    {
        try
        {
            LabyrinthParameters? parameters = await localStorage.GetItemAsync<LabyrinthParameters>(LabyrinthParameters.LocalStorageKey);

            // TODO: разовая миграция без версии настроек – заводить версионирование, когда сменится следующий дефолт
            if (parameters?.Color == LabyrinthParameters.LegacyDefaultColor)
            {
                parameters.Color = LabyrinthParameters.DefaultColor;
                await localStorage.SetItemAsync(LabyrinthParameters.LocalStorageKey, parameters);
            }

            return parameters;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Не удалось прочитать параметры лабиринта, остаются значения по умолчанию");
            return null;
        }
    }
}
