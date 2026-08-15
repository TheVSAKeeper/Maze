using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;

namespace Labirint.Web.Parameters;

public static class LabyrinthParametersLoader
{
    public static async Task ApplyAsync(ILocalStorageService localStorage, ILogger logger)
    {
        GlobalParameters.Labyrinth = await LoadAsync(localStorage, logger) ?? new LabyrinthParameters();
    }

    private static async Task<LabyrinthParameters?> LoadAsync(ILocalStorageService localStorage, ILogger logger)
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
