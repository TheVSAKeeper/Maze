namespace Labirint.Web.Components;

public partial class MazeWalls : MazeComponent
{
    private int _width;
    private int _height;

    protected override string CanvasId => "mazeWallsCanvas";

    protected override void OnParametersSetInner()
    {
        _width = WallWidth;
        _height = BoxSize + WallWidth;
    }

    protected override void DrawInner(int x, int y, DrawSequence sequence)
    {
        var topLeft = Vision.GetDraw((x, y)) * BoxSize;
        var bottomRight = topLeft + BoxSize;

        if (Maze[x, y].ContainsWall(Direction.Left))
        {
            sequence.DrawRect(topLeft.X, topLeft.Y, _width, _height);
        }

        if (Maze[x, y].ContainsWall(Direction.Top))
        {
            sequence.DrawRect(topLeft.X, topLeft.Y, _height, _width);
        }

        if (Maze[x, y].ContainsWall(Direction.Right))
        {
            sequence.DrawRect(bottomRight.X, topLeft.Y, _width, _height);
        }

        if (Maze[x, y].ContainsWall(Direction.Bottom))
        {
            sequence.DrawRect(topLeft.X, bottomRight.Y, _height, _width);
        }
    }
}
