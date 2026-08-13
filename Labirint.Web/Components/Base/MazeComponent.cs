using Labirint.Web.Parameters;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Components.Base;

public abstract class MazeComponent : RenderComponent, IDisposable
{
    protected ElementReference CanvasRef;

    private Canvas2DContext? _context;

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [CascadingParameter]
    public required MazeRenderParameters RenderParameters { get; set; }

    public int CanvasWidth { get; private set; }
    public int CanvasHeight { get; private set; }

    protected virtual string StrokeStyle => GlobalParameters.Labyrinth.Color;

    protected int BoxSize { get; private set; }
    protected int WallWidth { get; private set; }
    protected Labyrinth Maze { get; private set; } = null!;
    protected Vision Vision { get; private set; } = null!;

    protected abstract string CanvasId { get; }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }

    protected virtual void OnParametersSetInner()
    {
    }

    protected sealed override void OnParametersSet()
    {
        (Maze, BoxSize, WallWidth, Vision) = RenderParameters;

        // Данное замечание актуально, если в MazeWalls оставлять условия с исключением повторного рисования стен
        // и необходимо заменить перед прочтением 63 строку на данную: int renderRange = Vision.Range * 2 * BoxSize + WallWidth;
        // По факту рисуется Vision.Range * 2 * BoxSize + BoxSize + WallWidth,
        // но чтобы было (возможно) красивее оставлена только верхнюю часть ячейки (картинки были в предыдущем PR).
        // Из-за этого игрок размещается не в центре радиуса видимости.
        // Если данное поведение не устраивает, нужно заменить стоку 63 на закомментированную ниже.
        // int renderRange = Vision.Range * 2 * BoxSize + BoxSize + WallWidth;

        // Vision.Range * 2 -> область видимости во все стороны.
        // * BoxSize -> из относительного в абсолютное значение.
        // + BoxSize -> клетка с игроком.
        // + WallWidth -> для того, чтобы видеть стенки у клеток на границе обзора.

        var renderRange = Vision.Range * 2 * BoxSize + BoxSize + WallWidth;
        CanvasWidth = renderRange;
        CanvasHeight = renderRange;

        OnParametersSetInner();
    }

    protected override Task OnRenderAsyncInner()
    {
        DrawSequence drawSequence = new();
        drawSequence.ClearRect(0, 0, CanvasWidth, CanvasHeight);
        drawSequence.StrokeStyle(StrokeStyle);

        for (var x = Vision.Start.X; x <= Vision.Finish.X; x++)
        {
            for (var y = Vision.Start.Y; y <= Vision.Finish.Y; y++)
            {
                DrawInner(x, y, drawSequence);
            }
        }

        _context?.Draw(drawSequence);

        return Task.CompletedTask;
    }

    protected override Task OnFirstRenderAsyncInner()
    {
        var runtime = (IJSInProcessRuntime)JSRuntime;
        var contextRef = runtime.Invoke<IJSInProcessObjectReference>("canvasHelper.getContext2D", CanvasRef);

        _context = new(contextRef, runtime);

        return Task.CompletedTask;
    }

    protected abstract void DrawInner(int x, int y, DrawSequence sequence);
}
