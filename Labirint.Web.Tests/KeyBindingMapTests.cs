using Labirint.Core;
using Labirint.Web.Common.Control;
using Labirint.Web.Common.Control.Schemes;

namespace Labirint.Web.Tests;

[TestFixture]
public class KeyBindingMapTests
{
    [TestCase("Digit0", 0)]
    [TestCase("Digit7", 7)]
    [TestCase("Digit9", 9)]
    public void DigitIsRecognizedTest(string code, int expectedDigit)
    {
        Assert.That(KeyBindingMap.FindDigit(code), Is.EqualTo(expectedDigit));
    }

    [TestCase("Digit")]
    [TestCase("DigitA")]
    [TestCase("Numpad1")]
    [TestCase("KeyD")]
    [TestCase("digit1")]
    public void NotADigitReturnsNullTest(string code)
    {
        Assert.That(KeyBindingMap.FindDigit(code), Is.Null);
    }

    [Test]
    public void DirectionsAreBoundToSchemeTest()
    {
        ClassicScheme scheme = new();
        KeyBindingMap map = new();

        map.Rebuild(scheme, null);

        Assert.Multiple(() =>
        {
            Assert.That(map.FindDirection(scheme.MoveLeft), Is.EqualTo(Direction.Left));
            Assert.That(map.FindDirection(scheme.MoveUp), Is.EqualTo(Direction.Top));
            Assert.That(map.FindDirection(scheme.MoveRight), Is.EqualTo(Direction.Right));
            Assert.That(map.FindDirection(scheme.MoveDown), Is.EqualTo(Direction.Bottom));
        });
    }

    [Test]
    public void UnknownCodeHasNoBindingTest()
    {
        KeyBindingMap map = new();

        map.Rebuild(new ClassicScheme(), null);

        Assert.Multiple(() =>
        {
            Assert.That(map.FindDirection("F13"), Is.Null);
            Assert.That(map.FindItem("F13"), Is.Null);
        });
    }

    [Test]
    public void SchemeChangeReplacesDirectionsTest()
    {
        ClassicScheme classic = new();
        AlternativeScheme alternative = new();
        KeyBindingMap map = new();

        map.Rebuild(classic, null);
        map.Rebuild(alternative, null);

        var replacedCodes = new[] { classic.MoveLeft, classic.MoveUp, classic.MoveRight, classic.MoveDown }
            .Except([alternative.MoveLeft, alternative.MoveUp, alternative.MoveRight, alternative.MoveDown]);

        Assert.That(replacedCodes.Select(key => map.FindDirection(key)), Has.All.Null);
    }

    [Test]
    public void ItemsAreBoundToActivateKeysTest()
    {
        Inventory inventory = new();
        ClassicScheme scheme = new();
        KeyBindingMap map = new();

        map.Rebuild(scheme, inventory);

        var boundItems = inventory.AllItems.Where(item => item.ControlSettings != null).ToList();

        Assert.That(boundItems, Is.Not.Empty);

        Assert.That(boundItems.Select(item => map.FindItem(scheme.GetActivateKey(item.ControlSettings!).KeyCode)),
            Is.EqualTo(boundItems));
    }
}
