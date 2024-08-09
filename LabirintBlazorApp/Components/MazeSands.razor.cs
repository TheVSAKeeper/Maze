using LabirintBlazorApp.Common;
using LabirintBlazorApp.Dto;
using Microsoft.AspNetCore.Components;

namespace LabirintBlazorApp.Components;

public partial class MazeSands : CanvasComponent
{
    [Parameter]
    [EditorRequired]
    public required int[,] Sand { get; set; }

    [Parameter]
    [EditorRequired]
    public required int HalfBoxSize { get; set; }

    [Parameter]
    [EditorRequired]
    public required int WallWidth { get; set; }

    [Parameter]
    [EditorRequired]
    public required Vision Vision { get; set; }

    private int MazeWidth => Sand.GetLength(0);
    private int MazeHeight => Sand.GetLength(1);

    protected override int CanvasWidth => MazeWidth * HalfBoxSize;
    protected override int CanvasHeight => MazeHeight * HalfBoxSize;

    protected override string CanvasId => "mazeSandsCanvas";

    protected override async Task DrawAsync()
    {
        DrawSequence drawSequence = new();

        drawSequence.ClearRect(0, 0, CanvasWidth, CanvasHeight);

        int entityBoxSize = HalfBoxSize * 2 - WallWidth;
        int offset = HalfBoxSize - entityBoxSize;

        for (int y = Vision.Start.Y; y < Vision.Finish.Y; y += 2)
        {
            for (int x = Vision.Start.X; x < Vision.Finish.X; x += 2)
            {
                if (Sand[y, x] != 0)
                {
                    continue;
                }

                (int drawX, int drawY) = Vision.GetDraw(x, y);

                int left = (drawX - 1) * HalfBoxSize + WallWidth - offset / 2;
                int top = (drawY - 1) * HalfBoxSize + WallWidth - offset;

                drawSequence.DrawImage("/images/sand.png", left, top, HalfBoxSize, HalfBoxSize);
            }
        }

        Logger.LogInformation("Количество команд на отрисовку (sand): {Count}", drawSequence.Count);

        await Context.DrawSequenceAsync(drawSequence);
    }
}
