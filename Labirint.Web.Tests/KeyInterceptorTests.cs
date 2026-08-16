using Bunit;
using Labirint.Core;
using Labirint.Core.Items;
using Labirint.Core.Items.Base;
using Labirint.Web.Common.Control;
using Labirint.Web.Common.Control.Schemes;
using Labirint.Web.Components;
using Labirint.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Labirint.Web.Tests;

[TestFixture]
public class KeyInterceptorTests
{
    private BunitContext _context = null!;
    private Inventory _inventory = null!;
    private ClassicScheme _scheme = null!;

    private Hammer _hammer = null!;
    private Bomb _bomb = null!;

    [SetUp]
    public void SetUp()
    {
        _context = new();
        _context.JSInterop.Mode = JSRuntimeMode.Loose;
        _context.Services.AddLogging();
        _context.Services.AddSingleton<LocalStorageService>();
        _context.Services.AddSingleton<ControlSchemeService>();

        _inventory = new();
        _scheme = new();

        _hammer = _inventory.AllItems.OfType<Hammer>().Single();
        _bomb = _inventory.AllItems.OfType<Bomb>().Single();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _inventory.Clear();
    }

    [Test]
    public void PausedInterceptorIgnoresKeyMoveTest()
    {
        var rendered = RenderInterceptor();
        var moveCount = 0;

        rendered.Instance.MoveKeyDown += (_, _) => moveCount++;

        rendered.Find("button").Click();
        rendered.Instance.OnKeyDown(_scheme.MoveLeft.KeyCode);

        Assert.Multiple(() =>
        {
            Assert.That(rendered.Instance.IsPaused, Is.True);
            Assert.That(moveCount, Is.Zero);
        });
    }

    [Test]
    public void PausedInterceptorIgnoresSwipeTest()
    {
        var rendered = RenderInterceptor();
        var moveCount = 0;

        rendered.Instance.MoveKeyDown += (_, _) => moveCount++;

        rendered.Find("button").Click();
        rendered.Instance.OnKeyDown(Direction.Left);

        Assert.That(moveCount, Is.Zero);
    }

    [Test]
    public void ResumedInterceptorAcceptsSwipeTest()
    {
        var rendered = RenderInterceptor();
        var moveCount = 0;

        rendered.Instance.MoveKeyDown += (_, _) => moveCount++;

        rendered.Find("button").Click();
        rendered.Find("button").Click();
        rendered.Instance.OnKeyDown(Direction.Left);

        Assert.Multiple(() =>
        {
            Assert.That(rendered.Instance.IsPaused, Is.False);
            Assert.That(moveCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void MoveRequiredItemWaitsForDirectionTest()
    {
        var rendered = RenderInterceptor();

        AttackEventArgs? attack = null;
        Item? waitItem = null;
        var moveCount = 0;

        rendered.Instance.AttackKeyDown += (_, args) => attack = args;
        rendered.Instance.ChangedWaitItem += (_, item) => waitItem = item;
        rendered.Instance.MoveKeyDown += (_, _) => moveCount++;

        rendered.Instance.OnKeyDown(_scheme.GetActivateKey(_hammer.ControlSettings!).KeyCode);

        Assert.Multiple(() =>
        {
            Assert.That(waitItem, Is.SameAs(_hammer));
            Assert.That(attack, Is.Null);
        });

        rendered.Instance.OnKeyDown(_scheme.MoveLeft.KeyCode);

        Assert.Multiple(() =>
        {
            Assert.That(attack?.Item, Is.SameAs(_hammer));
            Assert.That(attack?.Direction, Is.EqualTo(Direction.Left));
            Assert.That(waitItem, Is.Null);
            Assert.That(moveCount, Is.Zero);
        });
    }

    [Test]
    public void InstantItemAttacksWithoutDirectionTest()
    {
        var rendered = RenderInterceptor();

        AttackEventArgs? attack = null;
        rendered.Instance.AttackKeyDown += (_, args) => attack = args;

        rendered.Instance.OnKeyDown(_scheme.GetActivateKey(_bomb.ControlSettings!).KeyCode);

        Assert.Multiple(() =>
        {
            Assert.That(attack?.Item, Is.SameAs(_bomb));
            Assert.That(attack?.Direction, Is.Null);
        });
    }

    [Test]
    public void ResetWaitItemReturnsMovesTest()
    {
        var rendered = RenderInterceptor();

        Item? waitItem = _hammer;
        var moveCount = 0;
        var attackCount = 0;

        rendered.Instance.ChangedWaitItem += (_, item) => waitItem = item;
        rendered.Instance.MoveKeyDown += (_, _) => moveCount++;
        rendered.Instance.AttackKeyDown += (_, _) => attackCount++;

        rendered.Instance.OnKeyDown(_scheme.GetActivateKey(_hammer.ControlSettings!).KeyCode);
        rendered.Instance.ResetWaitItem();
        rendered.Instance.OnKeyDown(_scheme.MoveLeft.KeyCode);

        Assert.Multiple(() =>
        {
            Assert.That(waitItem, Is.Null);
            Assert.That(moveCount, Is.EqualTo(1));
            Assert.That(attackCount, Is.Zero);
        });
    }

    [Test]
    public void EmptyItemDoesNotWaitForDirectionTest()
    {
        Labyrinth labyrinth = new(new SeedRandom(1));
        labyrinth.Init(16, 16, 40, _inventory.AllItems);

        while (_inventory.CanUse(_bomb))
        {
            _inventory.Use(_bomb, new(0, 0), Direction.Right, labyrinth);
        }

        var rendered = RenderInterceptor();

        var attackCount = 0;
        var waitCount = 0;

        rendered.Instance.AttackKeyDown += (_, _) => attackCount++;
        rendered.Instance.ChangedWaitItem += (_, _) => waitCount++;

        rendered.Instance.OnKeyDown(_scheme.GetActivateKey(_bomb.ControlSettings!).KeyCode);

        Assert.Multiple(() =>
        {
            Assert.That(attackCount, Is.Zero);
            Assert.That(waitCount, Is.Zero);
        });
    }

    private IRenderedComponent<KeyInterceptor> RenderInterceptor()
    {
        return _context.Render<KeyInterceptor>(parameters => parameters.Add(interceptor => interceptor.Inventory, _inventory));
    }
}
