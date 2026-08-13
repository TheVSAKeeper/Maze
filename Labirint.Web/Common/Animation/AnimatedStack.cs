using System.Collections.Concurrent;

namespace Labirint.Web.Common.Animation;

public class AnimatedStack(ItemStack stack)
{
    private readonly ConcurrentQueue<State> _stateQueue = new();
    private State _executedState = State.Removed;

    public static event Action<State>? StateChanged;

    public enum State
    {
        None = 0,
        Added = 1,
        Used = 2,
        CantAdd = 3,
        Waiting = 4,
        Removed = 5,
    }

    public ItemStack Stack { get; } = stack;

    public bool IsIdle => _stateQueue.IsEmpty && ExecutedState == State.Removed;

    private State ExecutedState
    {
        get => _executedState;
        set
        {
            if (_executedState == value)
            {
                return;
            }

            _executedState = value;
            StateChanged?.Invoke(_executedState);
        }
    }

    public string GetAnimation()
    {
        if (ExecutedState is State.Removed && _stateQueue.TryDequeue(out var state))
        {
            ExecutedState = state;
            ScheduleRemove(state);
        }

        return ExecutedState.ToAnimation();
    }

    public int GetDuration()
    {
        return ExecutedState.ToDuration();
    }

    public void AddState(State state)
    {
        if (_stateQueue.Contains(state) || ExecutedState == state)
        {
            return;
        }

        _stateQueue.Enqueue(state);
        StateChanged?.Invoke(_executedState);
    }

    public void RemoveState()
    {
        ExecutedState = State.Removed;
    }

    private void ScheduleRemove(State state)
    {
        if (state.IsRepeating() || state.ToDuration() <= 0)
        {
            return;
        }

        _ = RemoveAfterAsync(state);
    }

    private async Task RemoveAfterAsync(State state)
    {
        await Task.Delay(state.ToDuration());

        if (ExecutedState == state)
        {
            RemoveState();
        }
    }
}
