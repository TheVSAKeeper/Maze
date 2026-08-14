namespace Labirint.Web.Parameters;

public record MazeRenderParameters(Labyrinth Maze, int BoxSize, int WallWidth, Vision Vision)
{
    // Vision.Range * 2 -> область видимости во все стороны.
    // * BoxSize -> из относительного в абсолютное значение.
    // + BoxSize -> клетка с игроком.
    // + WallWidth -> для того, чтобы видеть стенки у клеток на границе обзора.
    public int RenderRange { get; } = Vision.Range * 2 * BoxSize + BoxSize + WallWidth;
}
