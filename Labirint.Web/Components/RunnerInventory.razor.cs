using Labirint.Web.Common.Animation;
using Labirint.Web.Common.Control.Schemes;
using Labirint.Web.Components.Dialogs;
using Labirint.Web.Services.Dialogs;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Components;

public partial class RunnerInventory : RenderComponent, IDisposable
{
    private Dictionary<Item, AnimatedStack> _stackCache = new();
    private Item? _pendingFlight;
    private Item? _pendingCast;
    private Item? _castingItem;

    [Parameter]
    [EditorRequired]
    public required Inventory Inventory { get; set; }

    [Parameter]
    [EditorRequired]
    public required KeyInterceptor Interceptor { get; set; }

    [Inject]
    public required ControlSchemeService SchemeService { get; set; }

    [Inject]
    public required PickupFlightService PickupFlight { get; set; }

    [Inject]
    public required DialogService DialogService { get; set; }

    private AnimatedStack? WaitItem { get; set; }

    private IControlScheme ControlScheme => SchemeService.CurrentScheme;

    public void Dispose()
    {
        UnsubscribeEvents();
        GC.SuppressFinalize(this);
    }

    public bool TryStartCast(Item item)
    {
        if (_castingItem != null || _stackCache.TryGetValue(item, out var stack) == false)
        {
            return false;
        }

        _castingItem = item;
        _pendingCast = item;

        AddStackAnimation(item, AnimatedStack.State.Used);
        stack.ReserveCount();

        RunSafe(ForceRenderAsync);
        return true;
    }

    public void CancelCast(Item item)
    {
        if (_castingItem != item)
        {
            return;
        }

        _castingItem = null;

        if (_stackCache.TryGetValue(item, out var stack))
        {
            stack.SyncCount();
        }
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

    protected override async Task OnAfterRenderAsyncInner(bool firstRender)
    {
        if (_pendingFlight != null)
        {
            var item = _pendingFlight;
            _pendingFlight = null;

            await PickupFlight.FlyAsync(item);
        }

        if (_pendingCast != null)
        {
            var item = _pendingCast;
            _pendingCast = null;

            await PickupFlight.CastAsync(item);
        }
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

        _pendingFlight = item;
        RunSafe(ForceRenderAsync);
    }

    private void OnItemCantAdded(object? sender, Item item)
    {
        AddStackAnimation(item, AnimatedStack.State.CantAdd);
    }

    private void OnItemUsed(object? sender, Item item)
    {
        if (_castingItem == item)
        {
            _castingItem = null;

            if (_stackCache.TryGetValue(item, out var casted))
            {
                casted.SyncCount();
            }

            RunSafe(ForceRenderAsync);
            return;
        }

        AddStackAnimation(item, AnimatedStack.State.Used);

        _pendingCast = item;
        RunSafe(ForceRenderAsync);
    }

    private void OnItemCantUsed(object? sender, Item item)
    {
        CancelCast(item);
        AddStackAnimation(item, AnimatedStack.State.CantUse);
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

    private async Task ShowLoreAsync(Item item)
    {
        if (_stackCache.TryGetValue(item, out var animatedStack) == false)
        {
            return;
        }

        DialogParameters parameters = new()
        {
            [nameof(ItemLoreDialog.Item)] = item,
            [nameof(ItemLoreDialog.Count)] = animatedStack.Stack.Count,
            [nameof(ItemLoreDialog.IsInfinite)] = animatedStack.Stack.IsInfinite,
        };

        var result = await DialogService.ShowAsync<ItemLoreDialog>(item.DisplayName, parameters, new DialogOptions
        {
            Width = DialogWidth.Large,
        });

        if (result.GetValue<bool>())
        {
            OnClicked(item);
        }
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
        Inventory.ItemCantUsed += OnItemCantUsed;
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
        Inventory.ItemCantUsed -= OnItemCantUsed;
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
