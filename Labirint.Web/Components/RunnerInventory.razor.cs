using Labirint.Web.Common.Animation;
using Labirint.Web.Common.Control.Schemes;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Components;

public partial class RunnerInventory : RenderComponent, IDisposable
{
    private Dictionary<Item, AnimatedStack> _stackCache = new();
    private bool _showDescription;
    private Item? _currentItem;

    [Parameter]
    [EditorRequired]
    public required Inventory Inventory { get; set; }

    [Parameter]
    [EditorRequired]
    public required KeyInterceptor Interceptor { get; set; }

    [Inject]
    public required ControlSchemeService SchemeService { get; set; }

    private AnimatedStack? WaitItem { get; set; }

    private IControlScheme ControlScheme => SchemeService.CurrentScheme;

    public void Dispose()
    {
        UnsubscribeEvents();
        GC.SuppressFinalize(this);
    }

    protected override Task OnFirstRenderAsyncInner()
    {
        InitializeItems();
        SubscribeEvents();

        return Task.CompletedTask;
    }

    protected override Task OnRenderAsyncInner()
    {
        return Task.CompletedTask;
    }

    private void OnSchemeChanged(object? sender, IControlScheme scheme)
    {
        RunSafe(ForceRenderAsync);
    }

    private void OnChangedWaitItem(object? sender, Item? item)
    {
        if (item == null)
        {
            WaitItem?.RemoveState();
            WaitItem = null;
            return;
        }

        if (_stackCache.TryGetValue(item, out var stack) == false)
        {
            return;
        }

        stack.AddState(AnimatedStack.State.Waiting);
        WaitItem = stack;
    }

    private void OnItemAdded(object? sender, Item item)
    {
        AddStackAnimation(item, AnimatedStack.State.Added);
    }

    private void OnItemCantAdded(object? sender, Item item)
    {
        AddStackAnimation(item, AnimatedStack.State.CantAdd);
    }

    private void OnItemUsed(object? sender, Item item)
    {
        AddStackAnimation(item, AnimatedStack.State.Used);
    }

    private void OnInventoryCleared(object? sender, EventArgs e)
    {
        InitializeItems();
        RunSafe(ForceRenderAsync);
    }

    private void OnAnimateStateChanged(AnimatedStack.State state)
    {
        RunSafe(ForceRenderAsync);
    }

    private void OnDigitKeyDown(object? sender, DigitEventArgs args)
    {
        var control = Inventory.Stacks
            .Where(stack => stack.Count > 0)
            .ElementAtOrDefault(args.Digit - 1)
            ?.Item.ControlSettings;

        if (control != null)
        {
            Interceptor.OnKeyDown(control.ActivateKey);
        }
    }

    private async Task ShowDescription(Item item)
    {
        _showDescription = true;
        _currentItem = item;
        await ForceRenderAsync();
    }

    private async Task HideDescription()
    {
        _showDescription = false;
        _currentItem = null;
        await ForceRenderAsync();
    }

    private void OnClicked(Item item)
    {
        if (item.ControlSettings != null)
        {
            Interceptor.OnKeyDown(ControlScheme.GetActivateKey(item.ControlSettings));
        }
    }

    private void InitializeItems()
    {
        _stackCache = Inventory.Stacks.ToDictionary(stack => stack.Item, stack => new AnimatedStack(stack));
    }

    private void SubscribeEvents()
    {
        Interceptor.ChangedWaitItem += OnChangedWaitItem;
        Interceptor.DigitKeyDown += OnDigitKeyDown;
        SchemeService.ControlSchemeChanged += OnSchemeChanged;
        AnimatedStack.StateChanged += OnAnimateStateChanged;

        Inventory.ItemAdded += OnItemAdded;
        Inventory.ItemCantAdded += OnItemCantAdded;
        Inventory.ItemUsed += OnItemUsed;
        Inventory.InventoryCleared += OnInventoryCleared;
    }

    private void UnsubscribeEvents()
    {
        Interceptor.ChangedWaitItem -= OnChangedWaitItem;
        Interceptor.DigitKeyDown -= OnDigitKeyDown;
        SchemeService.ControlSchemeChanged -= OnSchemeChanged;
        AnimatedStack.StateChanged -= OnAnimateStateChanged;

        Inventory.ItemAdded -= OnItemAdded;
        Inventory.ItemCantAdded -= OnItemCantAdded;
        Inventory.ItemUsed -= OnItemUsed;
        Inventory.InventoryCleared -= OnInventoryCleared;
    }

    private void AddStackAnimation(Item item, AnimatedStack.State animation)
    {
        if (_stackCache.TryGetValue(item, out var stack) == false)
        {
            return;
        }

        stack.AddState(animation);
    }
}
