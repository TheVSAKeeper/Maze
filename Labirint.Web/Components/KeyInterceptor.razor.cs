﻿using Labirint.Web.Common.Control.Schemes;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Components;

public partial class KeyInterceptor : IAsyncDisposable
{
    private bool _isPause = false;

    private Dictionary<string, Direction> _moveDirections = new();
    private Dictionary<string, Item> _itemUsed = new();

    private DotNetObjectReference<KeyInterceptor>? _reference;
    private Item? _waitItem;

    public event EventHandler<AttackEventArgs>? AttackKeyDown;
    public event EventHandler<Item?>? ChangedWaitItem;
    public event EventHandler<DigitEventArgs>? DigitKeyDown;
    public event EventHandler<MoveEventArgs>? MoveKeyDown;

    [Parameter]
    public required Inventory? Inventory { get; set; }

    [Inject]
    public required IControlSchemeService SchemeService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    private IControlScheme ControlScheme => SchemeService.CurrentScheme;

    public async ValueTask DisposeAsync()
    {
        await JSRuntime.InvokeVoidAsync("finalizeKeyInterceptor");
        _reference?.Dispose();

        SchemeService.ControlSchemeChanged -= OnSchemeChanged;
        GC.SuppressFinalize(this);
    }

    public void InitializeItems()
    {
        if (Inventory != null)
        {
            _itemUsed = Inventory.AllItems
                .Where(item => item.ControlSettings != null)
                .Select(item => (ControlScheme.GetActivateKey(item.ControlSettings!).KeyCode, item))
                .ToDictionary();
        }
    }

    [JSInvokable]
    public void OnKeyDown(string code)
    {
        if (_isPause)
        {
            return;
        }

        if (_waitItem == null && PerformItemUse(code, out AttackEventArgs? attack) && attack != null)
        {
            AttackKeyDown?.Invoke(this, attack);
        }

        if (PerformMove(code, out MoveEventArgs? move) && move != null)
        {
            PerformMove(move);
        }

        if (PerformDigitKey(code, out DigitEventArgs? digit) && digit != null)
        {
            DigitKeyDown?.Invoke(this, digit);
        }
    }

    public void OnKeyDown(Direction direction)
    {
        PerformMove(new MoveEventArgs
        {
            Direction = direction
        });
    }

    protected override void OnParametersSet()
    {
        InitializeItems();
    }

    protected override void OnInitialized()
    {
        _reference = DotNetObjectReference.Create(this);

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

    private void PerformMove(MoveEventArgs move)
    {
        if (_waitItem != null)
        {
            AttackKeyDown?.Invoke(this, new AttackEventArgs
            {
                Item = _waitItem,
                Direction = move.Direction
            });

            ChangeWaitItem(null);
        }

        MoveKeyDown?.Invoke(this, move);
    }

    private void Initialize()
    {
        InitializeItems();

        _moveDirections = new Dictionary<string, Direction>
        {
            [ControlScheme.MoveLeft] = Direction.Left,
            [ControlScheme.MoveUp] = Direction.Top,
            [ControlScheme.MoveRight] = Direction.Right,
            [ControlScheme.MoveDown] = Direction.Bottom
        };
    }

    private void ChangeWaitItem(Item? item)
    {
        _waitItem = item;
        ChangedWaitItem?.Invoke(this, item);
    }

    private bool PerformItemUse(string code, out AttackEventArgs? args)
    {
        args = null;

        if ((_itemUsed.TryGetValue(code, out Item? item) && (Inventory?.CanUse(item) ?? false)) == false)
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
            Item = item
        };

        return true;
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
            Direction = direction
        };

        return true;
    }

    private bool PerformDigitKey(string code, out DigitEventArgs? args)
    {
        args = null;

        if ((code.StartsWith("Digit") && char.IsDigit(code[^1])) == false)
        {
            return false;
        }

        args = new DigitEventArgs
        {
            Digit = code[^1] - '0'
        };

        return true;
    }
}
