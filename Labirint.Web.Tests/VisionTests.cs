namespace Labirint.Web.Tests;

[TestFixture]
public class VisionTests
{
    private const int MazeWidth = 16;
    private const int MazeHeight = 16;

    [TestCase(0, 0)]
    [TestCase(15, 15)]
    [TestCase(7, 7)]
    [TestCase(0, 15)]
    public void RunnerStaysInFrameCenterTest(int x, int y)
    {
        Vision vision = new(MazeWidth, MazeHeight);

        vision.SetPosition((x, y));

        Assert.That(vision.GetDraw(vision.Runner), Is.EqualTo(new Position(vision.Range, vision.Range)));
    }

    [TestCase(0, 0, -3, -3)]
    [TestCase(15, 15, 12, 12)]
    [TestCase(7, 7, 4, 4)]
    public void OriginIsNotClampedTest(int x, int y, int expectedOriginX, int expectedOriginY)
    {
        Vision vision = new(MazeWidth, MazeHeight);

        vision.SetPosition((x, y));

        Assert.That(vision.Origin, Is.EqualTo(new Position(expectedOriginX, expectedOriginY)));
    }

    [TestCase(0, 0, 0, 0, 3, 3)]
    [TestCase(15, 15, 12, 12, 15, 15)]
    [TestCase(7, 7, 4, 4, 10, 10)]
    [TestCase(0, 15, 0, 12, 3, 15)]
    public void StartAndFinishAreClampedToMazeTest(int x, int y, int expectedStartX, int expectedStartY, int expectedFinishX, int expectedFinishY)
    {
        Vision vision = new(MazeWidth, MazeHeight);

        vision.SetPosition((x, y));

        Assert.Multiple(() =>
        {
            Assert.That(vision.Start, Is.EqualTo(new Position(expectedStartX, expectedStartY)));
            Assert.That(vision.Finish, Is.EqualTo(new Position(expectedFinishX, expectedFinishY)));
        });
    }

    [Test]
    public void SmallMazeIsFullyVisibleTest()
    {
        Vision vision = new(2, 2);

        vision.SetPosition((0, 0));

        Assert.Multiple(() =>
        {
            Assert.That(vision.Start, Is.EqualTo(new Position(0, 0)));
            Assert.That(vision.Finish, Is.EqualTo(new Position(1, 1)));
            Assert.That(vision.GetDraw((1, 1)), Is.EqualTo(new Position(4, 4)));
        });
    }
}
