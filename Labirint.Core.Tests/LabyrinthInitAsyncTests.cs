namespace Labirint.Core.Tests;

[TestFixture]
public class LabyrinthInitAsyncTests
{
    private const int Seed = 7;

    [TestCase(16, 40)]
    [TestCase(64, 60)]
    public async Task AsyncInitRepeatsSyncInitTest(int size, int density)
    {
        Labyrinth expected = new(new SeedRandom(Seed));
        Labyrinth actual = new(new SeedRandom(Seed));

        expected.Init(size, size, density);
        await actual.InitAsync(size, size, density);

        Assert.That(Snapshot(actual), Is.EqualTo(Snapshot(expected)));
    }

    [Test]
    public async Task ProgressIsReportedUpToHundredTest()
    {
        Labyrinth labyrinth = new(new SeedRandom(Seed));
        ProgressCollector progress = new();

        await labyrinth.InitAsync(200, 200, 40, progress: progress);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Values, Is.Ordered);
            Assert.That(progress.Values, Has.Count.GreaterThan(1));
            Assert.That(progress.Values.Last(), Is.EqualTo(100));
        });
    }

    [Test]
    public async Task SmallMazeIsGeneratedInSinglePassTest()
    {
        Labyrinth labyrinth = new(new SeedRandom(Seed));
        ProgressCollector progress = new();

        await labyrinth.InitAsync(16, 16, 40, progress: progress);

        Assert.That(progress.Values, Is.EqualTo(new[] { 100 }));
    }

    private static (Direction Walls, bool IsExit)[] Snapshot(Labyrinth labyrinth)
    {
        var tiles = new (Direction, bool)[labyrinth.Width * labyrinth.Height];

        for (var x = 0; x < labyrinth.Width; x++)
        {
            for (var y = 0; y < labyrinth.Height; y++)
            {
                tiles[x * labyrinth.Height + y] = (labyrinth[x, y].Walls, labyrinth[x, y].IsExit);
            }
        }

        return tiles;
    }

    private sealed class ProgressCollector : IProgress<int>
    {
        public List<int> Values { get; } = [];

        public void Report(int value)
        {
            Values.Add(value);
        }
    }
}
