using Labirint.Core.TileFeatures.Common;

namespace Labirint.Web.Tests;

[TestFixture]
public class AlignmentHelperTests
{
    private const int BoxSize = 64;
    private const int WallWidth = 6;
    private const double Scale = 0.9;

    [TestCase(Alignment.TopLeft, 70, 70)]
    [TestCase(Alignment.TopCenter, 73, 70)]
    [TestCase(Alignment.TopRight, 76, 70)]
    [TestCase(Alignment.CenterLeft, 70, 73)]
    [TestCase(Alignment.Center, 73, 73)]
    [TestCase(Alignment.CenterRight, 76, 73)]
    [TestCase(Alignment.BottomLeft, 70, 76)]
    [TestCase(Alignment.BottomCenter, 73, 76)]
    [TestCase(Alignment.BottomRight, 76, 76)]
    public void ScaledEntityIsShiftedByAlignmentTest(Alignment alignment, int expectedX, int expectedY)
    {
        var (entitySize, drawPosition) = AlignmentHelper.GetAlignmentParameters(BoxSize, WallWidth, Scale, (1, 1), alignment);

        Assert.Multiple(() =>
        {
            Assert.That(entitySize, Is.EqualTo(52));
            Assert.That(drawPosition, Is.EqualTo(new Position(expectedX, expectedY)));
        });
    }

    [Test]
    public void StretchIgnoresWallWidthTest()
    {
        var (entitySize, drawPosition) = AlignmentHelper.GetAlignmentParameters(BoxSize, WallWidth, Scale, (1, 1), Alignment.Stretch);

        Assert.Multiple(() =>
        {
            Assert.That(entitySize, Is.EqualTo(57));
            Assert.That(drawPosition, Is.EqualTo(new Position(73, 73)));
        });
    }

    [TestCase(Alignment.TopLeft)]
    [TestCase(Alignment.Center)]
    [TestCase(Alignment.BottomRight)]
    public void FullScaleKeepsCellCornerTest(Alignment alignment)
    {
        var (entitySize, drawPosition) = AlignmentHelper.GetAlignmentParameters(BoxSize, WallWidth, 1, (2, 3), alignment);

        Assert.Multiple(() =>
        {
            Assert.That(entitySize, Is.EqualTo(BoxSize - WallWidth));
            Assert.That(drawPosition, Is.EqualTo(new Position(2 * BoxSize + WallWidth, 3 * BoxSize + WallWidth)));
        });
    }

    [Test]
    public void UnknownAlignmentThrowsTest()
    {
        Assert.That(() => AlignmentHelper.GetAlignmentParameters(BoxSize, WallWidth, Scale, (0, 0), (Alignment)42),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
