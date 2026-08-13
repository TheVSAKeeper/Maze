using System.Text;
using Labirint.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Components;

public partial class HeroMaze : IDisposable
{
    private const int Size = 13;
    private const int Cell = 24;
    private const int Side = Size * Cell;
    private const int Density = 42;
    private const int ExitX = Size / 2;
    private const int ExitY = Size - 1;

    private const int WallDrawStep = 9;
    private const int TrailStartPause = 400;
    private const int StepDuration = 45;
    private const int MinTrailDuration = 2200;
    private const int MaxTrailDuration = 7000;
    private const int CyclePause = 1200;
    private const int TailLength = 5 * Cell;

    private const int SolvableAttempts = 12;
    private const int SeedFloor = 10_000_000;
    private const int SeedCeiling = 100_000_000;

    private Run _run = null!;
    private int? _appliedSeed;
    private bool _isDisposed;
    private CancellationTokenSource _waitTokenSource = new();

    [Parameter]
    public int Seed { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private string RunStyle =>
        $"--route: path('{_run.TrailPath}'); --trail-length: {_run.TrailLength}; --tail-length: {TailLength}; " +
        $"--trail-delay: {_run.TrailDelay}ms; --trail-duration: {_run.TrailDuration}ms; " +
        $"--found-delay: {_run.TrailDelay + _run.TrailDuration}ms";

    public void Dispose()
    {
        _isDisposed = true;
        _waitTokenSource.Cancel();
        _waitTokenSource.Dispose();
    }

    protected override void OnParametersSet()
    {
        if (_appliedSeed == Seed)
        {
            return;
        }

        _appliedSeed = Seed;
        _run = BuildRun(Seed);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender == false)
        {
            return;
        }

        await CycleAsync();
    }

    private static Run BuildRun(int seed)
    {
        Run? unsolved = null;

        for (var attempt = 0; attempt < SolvableAttempts; attempt++)
        {
            var run = Compose(seed + attempt);

            if (run.IsSolved)
            {
                return run;
            }

            unsolved ??= run;
        }

        return unsolved!;
    }

    private static Run Compose(int seed)
    {
        Labyrinth labyrinth = new(new FixedRandom(seed));
        labyrinth.Init(Size, Size, Density, []);

        var walls = CollectWalls(labyrinth);
        var walk = Walk(labyrinth, seed, out var isSolved);

        var steps = walk.Count - 1;

        return new Run(
            seed,
            walls,
            BuildTrailPath(walk),
            steps * Cell,
            walls.Count * WallDrawStep + TrailStartPause,
            Math.Clamp(steps * StepDuration, MinTrailDuration, MaxTrailDuration),
            isSolved);
    }

    private static List<(int X1, int Y1, int X2, int Y2)> CollectWalls(Labyrinth labyrinth)
    {
        List<(int X1, int Y1, int X2, int Y2)> walls = [];

        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                var tile = labyrinth[x, y];

                if (tile.ContainsWall(Direction.Top))
                {
                    walls.Add((x * Cell, y * Cell, (x + 1) * Cell, y * Cell));
                }

                if (tile.ContainsWall(Direction.Left))
                {
                    walls.Add((x * Cell, y * Cell, x * Cell, (y + 1) * Cell));
                }

                if (x == Size - 1 && tile.ContainsWall(Direction.Right))
                {
                    walls.Add(((x + 1) * Cell, y * Cell, (x + 1) * Cell, (y + 1) * Cell));
                }

                if (y == Size - 1 && tile.ContainsWall(Direction.Bottom))
                {
                    walls.Add((x * Cell, (y + 1) * Cell, (x + 1) * Cell, (y + 1) * Cell));
                }
            }
        }

        return walls;
    }

    private static List<Position> Walk(Labyrinth labyrinth, int seed, out bool isSolved)
    {
        var visited = new bool[Size, Size];
        Position exit = new(ExitX, ExitY);
        Position start = new(0, 0);

        Stack<Position> stack = new();
        stack.Push(start);
        visited[start.X, start.Y] = true;

        List<Position> walk = [start];
        Random random = new(seed);

        isSolved = false;

        while (stack.Count > 0)
        {
            var current = stack.Peek();

            if (current == exit)
            {
                isSolved = true;
                break;
            }

            var next = FindStep(labyrinth, visited, current, random);

            if (next is null)
            {
                stack.Pop();

                if (stack.Count > 0)
                {
                    walk.Add(stack.Peek());
                }

                continue;
            }

            visited[next.Value.X, next.Value.Y] = true;
            stack.Push(next.Value);
            walk.Add(next.Value);
        }

        return walk;
    }

    private static Position? FindStep(Labyrinth labyrinth, bool[,] visited, Position current, Random random)
    {
        Span<Direction> directions = [Direction.Top, Direction.Right, Direction.Bottom, Direction.Left];
        random.Shuffle(directions);

        foreach (var direction in directions)
        {
            if (labyrinth[current.X, current.Y].ContainsWall(direction))
            {
                continue;
            }

            var candidate = current + direction;

            if (candidate.X < 0 || candidate.Y < 0 || candidate.X >= Size || candidate.Y >= Size)
            {
                continue;
            }

            if (visited[candidate.X, candidate.Y] == false)
            {
                return candidate;
            }
        }

        return null;
    }

    private static string BuildTrailPath(List<Position> walk)
    {
        StringBuilder builder = new();

        for (var index = 0; index < walk.Count; index++)
        {
            var point = walk[index];

            builder.Append(index == 0 ? 'M' : 'L')
                .Append(point.X * Cell + Cell / 2)
                .Append(' ')
                .Append(point.Y * Cell + Cell / 2)
                .Append(' ');
        }

        return builder.ToString().TrimEnd();
    }

    private async Task CycleAsync()
    {
        while (_isDisposed == false)
        {
            var token = _waitTokenSource.Token;

            try
            {
                await Task.Delay(_run.TrailDelay + _run.TrailDuration + CyclePause, token);
            }
            catch (OperationCanceledException)
            {
                continue;
            }

            if (IsPageHidden())
            {
                continue;
            }

            ShowNext();

            await InvokeAsync(StateHasChanged);
        }
    }

    private bool IsPageHidden()
    {
        if (JSRuntime is not IJSInProcessRuntime runtime)
        {
            return false;
        }

        try
        {
            return runtime.Invoke<bool>("labirintPage.isHidden");
        }
        catch (JSException)
        {
            return false;
        }
    }

    private void Regenerate()
    {
        ShowNext();

        var previous = _waitTokenSource;
        _waitTokenSource = new CancellationTokenSource();
        previous.Cancel();
        previous.Dispose();
    }

    private void ShowNext()
    {
        _run = BuildRun(Random.Shared.Next(SeedFloor, SeedCeiling));
    }

    private sealed record Run(
        int Seed,
        IReadOnlyList<(int X1, int Y1, int X2, int Y2)> Walls,
        string TrailPath,
        int TrailLength,
        int TrailDelay,
        int TrailDuration,
        bool IsSolved);

    private sealed class FixedRandom(int seed) : IRandom
    {
        public Random Generator { get; } = new(seed);
    }
}
