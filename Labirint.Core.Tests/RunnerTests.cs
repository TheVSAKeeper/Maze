using Labirint.Core.Abilities;

namespace Labirint.Core.Tests;

[TestFixture]
public class RunnerTests : LabyrinthTestsBase
{
    [TestCase(0, 0, Direction.Left)]
    [TestCase(0, 0, Direction.Top)]
    [TestCase(DefaultWidth - 1, DefaultHeight - 1, Direction.Right)]
    [TestCase(DefaultWidth - 1, DefaultHeight - 1, Direction.Bottom)]
    public void WallsIgnoringRunnerStopsAtBorderTest(int x, int y, Direction direction)
    {
        Runner runner = new((x, y), Labyrinth, new());
        runner.AddAbility(new WalkThroughWallsAbility());

        Assert.Multiple(() =>
        {
            Assert.That(runner.Move(direction), Is.False);
            Assert.That(runner.Position, Is.EqualTo(new Position(x, y)));
        });
    }

    [Test]
    public void RunnerStopsAtOpenExitTest()
    {
        Position exit = new(DefaultWidth / 2, DefaultHeight - 1);
        Runner runner = new(exit, Labyrinth, new());

        Assert.Multiple(() =>
        {
            Assert.That(Labyrinth[exit].ContainsWall(Direction.Bottom), Is.False);
            Assert.That(runner.Move(Direction.Bottom), Is.False);
            Assert.That(runner.Position, Is.EqualTo(exit));
        });
    }

    [Test]
    public void WallsIgnoringRunnerCrossesInnerWallTest()
    {
        Runner runner = new((1, 1), Labyrinth, new());
        Labyrinth.CreateWall((1, 1), Direction.Right);
        runner.AddAbility(new WalkThroughWallsAbility());

        Assert.Multiple(() =>
        {
            Assert.That(Labyrinth[1, 1].ContainsWall(Direction.Right), Is.True);
            Assert.That(runner.Move(Direction.Right), Is.True);
            Assert.That(runner.Position, Is.EqualTo(new Position(2, 1)));
        });
    }
}
