﻿using Labirint.Core.Common;
using LabirintBlazorApp.Common.Control.Schemes;
using Microsoft.AspNetCore.Components;

namespace LabirintBlazorApp.Components;

public partial class KeyInterceptor : IAsyncDisposable
{
    private bool _isPause = false;

    private Dictionary<string, Direction> _moveDirections = new();
    private Dictionary<string, Item?> _itemUsed = new();

    private DotNetObjectReference<KeyInterceptor>? _reference;
    private Item? _waitItem;

    public event EventHandler<Item?>? ChangedWaitItem;

    [Inject]
    public required IControlSchemeService SchemeService { get; set; }

    [Inject]
    public required InventoryService InventoryService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Parameter]
    public EventCallback<MoveEventArgs> OnMoveKeyDown { get; set; }

    [Parameter]
    public EventCallback<AttackEventArgs> OnAttackKeyDown { get; set; }

    private IControlScheme ControlScheme => SchemeService.CurrentScheme;

    public async ValueTask DisposeAsync()
    {
        await JSRuntime.InvokeVoidAsync("finalizeKeyInterceptor");
        _reference?.Dispose();

        SchemeService.ControlSchemeChanged -= OnSchemeChanged;
        GC.SuppressFinalize(this);
    }

    [JSInvokable]
    public async Task OnKeyDown(string code)
    {
        if (_isPause)
        {
            return;
        }

        if (PerformItemUse(code, out AttackEventArgs? attack) && _waitItem == null)
        {
            await OnAttackKeyDown.InvokeAsync(attack);
        }

        if (PerformMove(code, out MoveEventArgs? move))
        {
            if (_waitItem != null)
            {
                await OnAttackKeyDown.InvokeAsync(new AttackEventArgs
                {
                    Item = _waitItem,
                    Direction = move.Direction,
                    KeyCode = Key.Create(code)
                });

                ChangeWaitItem(null);
            }

            await OnMoveKeyDown.InvokeAsync(move);
        }
    }

    protected override void OnInitialized()
    {
        _reference = DotNetObjectReference.Create(this);

        _itemUsed = InventoryService.Items
            .Where(item => item.ControlSettings != null)
            .Select(item => (item.ControlSettings!.ActivateKey!.KeyCode, x: item))
            .ToDictionary()!;

        Initialize();
        SchemeService.ControlSchemeChanged += OnSchemeChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("initializeKeyInterceptor", _reference);
        }
    }

    private void OnSchemeChanged(object? sender, IControlScheme scheme)
    {
        Initialize();
        StateHasChanged();
    }

    private void Initialize()
    {
        _moveDirections = new Dictionary<string, Direction>
        {
            { ControlScheme.MoveLeft, Direction.Left },
            { ControlScheme.MoveUp, Direction.Top },
            { ControlScheme.MoveRight, Direction.Right },
            { ControlScheme.MoveDown, Direction.Bottom }
        };
    }

    private bool PerformItemUse(string code, out AttackEventArgs? args)
    {
        args = null;

        if (_itemUsed.TryGetValue(code, out Item? item) == false)
        {
            return false;
        }

        if (item.ControlSettings!.MoveRequired && _waitItem == null)
        {
            ChangeWaitItem(item);
            return false;
        }

        args = new AttackEventArgs
        {
            Item = item,
            KeyCode = Key.Create(code)
        };

        return true;
    }

    private void ChangeWaitItem(Item? item)
    {
        _waitItem = item;
        ChangedWaitItem?.Invoke(this, item);
    }

    private bool PerformMove(string code, out MoveEventArgs? args)
    {
        args = null;

        if (_moveDirections.TryGetValue(code, out Direction direction) == false)
        {
            return false;
        }

        args = new MoveEventArgs
        {
            Direction = direction,
            KeyCode = Key.Create(code)
        };

        return true;
    }
}
