namespace Labirint.Web.Tests;

[TestFixture]
public class DrawSequenceTests
{
    private const string Floor = "images/floor.png";
    private const string Wall = "images/wall.png";

    [Test]
    public void SpritesWithSameSourceCollapseTest()
    {
        DrawSequence sequence = new();

        sequence.DrawSprite(Floor, 0d, 0d, 16, 16, 0, 0, 64, 64);
        sequence.DrawSprite(Floor, 16d, 0d, 16, 16, 64, 0, 64, 64);

        var commands = sequence.Build();

        Assert.That(commands, Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(commands[0].Type, Is.EqualTo(DrawSequence.Command.DrawSprites));
            Assert.That(commands[0].Source, Is.EqualTo(Floor));
            Assert.That(commands[0].Sprites, Is.EqualTo(new double[] { 0, 0, 16, 16, 0, 0, 64, 64, 16, 0, 16, 16, 64, 0, 64, 64 }));
        });
    }

    [Test]
    public void SourceChangeFlushesSpritesTest()
    {
        DrawSequence sequence = new();

        sequence.DrawSprite(Floor, 0d, 0d, 16, 16, 0, 0, 64, 64);
        sequence.DrawSprite(Wall, 0d, 0d, 16, 16, 0, 0, 64, 64);

        var commands = sequence.Build();

        Assert.That(commands, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(commands[0].Source, Is.EqualTo(Floor));
            Assert.That(commands[0].Sprites, Has.Length.EqualTo(8));
            Assert.That(commands[1].Source, Is.EqualTo(Wall));
            Assert.That(commands[1].Sprites, Has.Length.EqualTo(8));
        });
    }

    [Test]
    public void OtherCommandFlushesSpritesBeforeItselfTest()
    {
        DrawSequence sequence = new();

        sequence.DrawSprite(Floor, 0d, 0d, 16, 16, 0, 0, 64, 64);
        sequence.Stroke();
        sequence.DrawSprite(Floor, 0d, 0d, 16, 16, 0, 0, 64, 64);

        var commands = sequence.Build();

        Assert.That(commands.Select(command => command.Type),
            Is.EqualTo(new[] { DrawSequence.Command.DrawSprites, DrawSequence.Command.Stroke, DrawSequence.Command.DrawSprites }));
    }

    [Test]
    public void RowAndColumnAreTranslatedToSourceOffsetTest()
    {
        DrawSequence sequence = new();

        sequence.DrawSprite(Floor, 2, 3, 10, 20, 100, 200, 64, 64);

        var commands = sequence.Build();

        Assert.That(commands[0].Sprites, Is.EqualTo(new double[] { 30, 40, 10, 20, 100, 200, 64, 64 }));
    }

    [Test]
    public void SquareSpriteKeepsSizeOnBothSidesTest()
    {
        DrawSequence sequence = new();

        sequence.DrawSprite(Floor, 1, 1, 128, 256, 64);

        var commands = sequence.Build();

        Assert.That(commands[0].Sprites, Is.EqualTo(new double[] { 64, 64, 64, 64, 128, 256, 64, 64 }));
    }

    [Test]
    public void EmptySequenceProducesNoCommandsTest()
    {
        DrawSequence sequence = new();

        Assert.That(sequence.Build(), Is.Empty);
    }
}
