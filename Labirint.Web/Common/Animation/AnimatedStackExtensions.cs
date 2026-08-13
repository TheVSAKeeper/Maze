namespace Labirint.Web.Common.Animation;

public static class AnimatedStackExtensions
{
    public static string ToAnimation(this AnimatedStack.State state)
    {
        return state switch
        {
            AnimatedStack.State.Added => "added-animate",
            AnimatedStack.State.Used => "used-animate",
            AnimatedStack.State.CantAdd => "max-count-animate",
            AnimatedStack.State.Waiting => "waiting-animate",
            AnimatedStack.State.Removed => string.Empty,
            AnimatedStack.State.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };
    }

    public static int ToDuration(this AnimatedStack.State state)
    {
        return state switch
        {
            AnimatedStack.State.Added => 500,
            AnimatedStack.State.Used => 500,
            AnimatedStack.State.CantAdd => 1000,
            AnimatedStack.State.Waiting => 1000,
            AnimatedStack.State.Removed => 0,
            AnimatedStack.State.None => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };
    }

    public static bool IsRepeating(this AnimatedStack.State state)
    {
        return state is AnimatedStack.State.Waiting;
    }
}
