using Labirint.Core;
using Labirint.Core.Items;
using Labirint.Web.Common.Animation;

namespace Labirint.Web.Tests;

[TestFixture]
public class AnimatedStackTests
{
    [Test]
    public void CancelDropsQueuedStateTest()
    {
        var stack = CreateStack();

        stack.AddState(AnimatedStack.State.Waiting);
        stack.GetAnimation();
        stack.AddState(AnimatedStack.State.Used);

        stack.CancelState();

        Assert.That(stack.GetAnimation(), Is.Empty);
    }

    [Test]
    public async Task FinishedStateGivesWayToQueuedStateTest()
    {
        var stack = CreateStack();

        stack.AddState(AnimatedStack.State.Used);
        stack.GetAnimation();
        stack.AddState(AnimatedStack.State.CantUse);

        await Task.Delay(AnimatedStack.State.Used.ToDuration() + AnimatedStack.State.Used.ToDelay() + 200);

        Assert.That(stack.GetAnimation(), Is.EqualTo(AnimatedStack.State.CantUse.ToAnimation()));
    }

    [Test]
    public void StateChangedIsRaisedByOwnStackOnlyTest()
    {
        var observed = CreateStack();
        var other = CreateStack();

        var count = 0;
        observed.StateChanged += _ => count++;

        other.AddState(AnimatedStack.State.Used);
        other.GetAnimation();

        Assert.That(count, Is.Zero);
    }

    private static AnimatedStack CreateStack()
    {
        return new(new ItemStack(new Hammer()));
    }
}
