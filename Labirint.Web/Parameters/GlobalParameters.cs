namespace Labirint.Web.Parameters;

public static class GlobalParameters
{
    private static LabyrinthParameters _labyrinth = new();

    public static event EventHandler? LabyrinthChanged;

    public static LabyrinthParameters Labyrinth
    {
        get => _labyrinth;
        set
        {
            _labyrinth = value;
            LabyrinthChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
