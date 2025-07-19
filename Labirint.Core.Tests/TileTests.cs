namespace Labirint.Core.Tests;

[TestFixture]
public class TileTests : LabyrinthTestsBase
{
    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            Direction[] directions = [Direction.None, Direction.Left, Direction.Top, Direction.Right, Direction.Bottom];

            var combinedDirections = directions.Where(direction => direction != Direction.None).GetCombinedDirections();

            foreach (var (direction, count) in combinedDirections)
            {
                foreach (var directionToAdd in directions)
                {
                    var expected = direction != Direction.All
                                   && direction != Direction.None
                                   && direction != directionToAdd
                                   && direction.HasFlag(directionToAdd) == false
                                   && count < 3;

                    yield return new(direction, directionToAdd, expected);
                }
            }
        }
    }

    [TestCaseSource(nameof(TestCases))]
    public void CanAddWallTest(Direction existingWalls, Direction directionToAdd, bool expectedResult)
    {
        Tile tile = new(Labyrinth)
        {
            Walls = existingWalls,
        };

        var result = tile.CanAddWall(directionToAdd);

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
