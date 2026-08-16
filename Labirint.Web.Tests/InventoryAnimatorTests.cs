using Labirint.Core;
using Labirint.Core.Items;
using Labirint.Core.Items.Base;
using Labirint.Web.Common.Animation;

namespace Labirint.Web.Tests;

[TestFixture]
public class InventoryAnimatorTests
{
    private Inventory _inventory = null!;
    private InventoryAnimator _animator = null!;
    private Labyrinth _labyrinth = null!;

    private Hammer _hammer = null!;
    private Bomb _bomb = null!;
    private WalkThroughWallsBottle _bottle = null!;

    [SetUp]
    public void SetUp()
    {
        _inventory = new();
        _animator = new(_inventory);

        _labyrinth = new(new SeedRandom(1));
        _labyrinth.Init(16, 16, 40, _inventory.AllItems);

        _hammer = _inventory.AllItems.OfType<Hammer>().Single();
        _bomb = _inventory.AllItems.OfType<Bomb>().Single();
        _bottle = _inventory.AllItems.OfType<WalkThroughWallsBottle>().Single();
    }

    [TearDown]
    public void TearDown()
    {
        _animator.Dispose();
        _inventory.Clear();
    }

    [Test]
    public void CastReservesCountTest()
    {
        var stack = _animator.Find(_hammer)!;
        var count = stack.Stack.Count;

        Assert.That(_animator.TryStartCast(_hammer), Is.True);
        Assert.That(stack.DisplayCount, Is.EqualTo(count - 1));
    }

    [Test]
    public void SecondCastIsRejectedWhileFirstIsInFlightTest()
    {
        _animator.TryStartCast(_hammer);

        Assert.Multiple(() =>
        {
            Assert.That(_animator.TryStartCast(_hammer), Is.False);
            Assert.That(_animator.TryStartCast(_bomb), Is.False);
        });
    }

    [Test]
    public void CancelCastRestoresCountTest()
    {
        var stack = _animator.Find(_hammer)!;
        var count = stack.Stack.Count;

        _animator.TryStartCast(_hammer);
        _animator.CancelCast(_hammer);

        Assert.Multiple(() =>
        {
            Assert.That(stack.DisplayCount, Is.EqualTo(count));
            Assert.That(_animator.TryStartCast(_bomb), Is.True);
        });
    }

    [Test]
    public void CancelCastOfAnotherItemKeepsCastTest()
    {
        var stack = _animator.Find(_hammer)!;
        var count = stack.Stack.Count;

        _animator.TryStartCast(_hammer);
        _animator.CancelCast(_bomb);

        Assert.Multiple(() =>
        {
            Assert.That(stack.DisplayCount, Is.EqualTo(count - 1));
            Assert.That(_animator.TryStartCast(_bomb), Is.False);
        });
    }

    [Test]
    public void UsedItemFinishesCastTest()
    {
        var stack = _animator.Find(_hammer)!;

        _animator.TryStartCast(_hammer);
        _inventory.Use(_hammer, new(0, 0), Direction.Right, _labyrinth);

        Assert.Multiple(() =>
        {
            Assert.That(stack.DisplayCount, Is.EqualTo(stack.Stack.Count));
            Assert.That(_animator.TryStartCast(_hammer), Is.True);
        });
    }

    [Test]
    public void EngineRefusalFinishesCastTest()
    {
        var stack = _animator.Find(_bottle)!;

        _animator.TryStartCast(_bottle);
        _inventory.Use(_bottle, new(0, 0), Direction.Right, _labyrinth);

        Assert.Multiple(() =>
        {
            Assert.That(stack.DisplayCount, Is.EqualTo(stack.Stack.Count));
            Assert.That(_animator.TryStartCast(_bottle), Is.True);
        });
    }

    [Test]
    public void WaitItemIsCancelledByNullTest()
    {
        var stack = _animator.Find(_hammer)!;

        _animator.SetWaitItem(_hammer);
        Assert.That(stack.GetAnimation(), Is.EqualTo(AnimatedStack.State.Waiting.ToAnimation()));

        _animator.SetWaitItem(null);
        Assert.That(stack.GetAnimation(), Is.Empty);
    }

    [Test]
    public void ClearedInventoryReleasesOldStacksTest()
    {
        var oldStack = _animator.Find(_hammer)!;

        _inventory.Clear();

        var changedCount = 0;
        _animator.Changed += () => changedCount++;

        oldStack.AddState(AnimatedStack.State.Used);

        Assert.Multiple(() =>
        {
            Assert.That(changedCount, Is.Zero);
            Assert.That(_animator.Find(_hammer), Is.Not.SameAs(oldStack));
        });
    }

    [Test]
    public void ClearedInventoryTracksNewStacksTest()
    {
        _inventory.Clear();

        var changedCount = 0;
        _animator.Changed += () => changedCount++;

        _animator.Find(_hammer)!.AddState(AnimatedStack.State.Used);

        Assert.That(changedCount, Is.Positive);
    }

    [Test]
    public void ClearedInventoryDropsCastTest()
    {
        _animator.TryStartCast(_hammer);
        _inventory.Clear();

        Assert.That(_animator.TryStartCast(_hammer), Is.True);
    }

    [Test]
    public void DisposedAnimatorIgnoresInventoryTest()
    {
        _animator.Dispose();

        var changedCount = 0;
        var flightCount = 0;

        _animator.Changed += () => changedCount++;
        _animator.PickupFlightRequested += _ => flightCount++;

        _inventory.TryAdd(_bottle);

        Assert.Multiple(() =>
        {
            Assert.That(changedCount, Is.Zero);
            Assert.That(flightCount, Is.Zero);
        });
    }

    [Test]
    public void PickedUpItemRequestsFlightTest()
    {
        Item? flownItem = null;
        _animator.PickupFlightRequested += item => flownItem = item;

        _inventory.TryAdd(_bottle);

        Assert.That(flownItem, Is.SameAs(_bottle));
    }
}
