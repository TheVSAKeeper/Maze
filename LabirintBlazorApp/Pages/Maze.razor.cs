﻿using Labirint.Core.TileFeatures.Base;
using LabirintBlazorApp.Components;
using LabirintBlazorApp.Parameters;
using Microsoft.AspNetCore.Components;

namespace LabirintBlazorApp.Pages;

public partial class Maze : IAsyncDisposable
{
    private const int MinSize = 1;
    private const int MaxSize = 500;

    private bool _isExitFound;
    private bool _isInit;

    private int _originalSize;
    private int _density;

    private int _boxSize;
    private int _wallWidth;

    private MazeFloor? _mazeFloor;
    private MazeWalls? _mazeWalls;
    private MazeEntities? _mazeEntities;
    private MazeRenderParameters? _renderParameter;
    private KeyInterceptor? _keyInterceptor;

    private Labyrinth _labyrinth = null!;
    private MazeSeed _seeder = null!;
    private Vision _vision = null!;

    [Parameter]
    public string? Seed { get; set; }

    [SupplyParameterFromQuery(Name = MazeSeed.SizeQueryName)]
    public int? MazeSize { get; set; }

    [SupplyParameterFromQuery(Name = MazeSeed.DensityQueryName)]
    public int? MazeDensity { get; set; }

    // Проверка на null и инициализацию (дополнительная проверка, если флаг выставили в true, а значение у не null полей не выставили)
    private bool IsInit => _isInit && _labyrinth != null && _seeder != null && _vision != null && _renderParameter != null;

    public async ValueTask DisposeAsync()
    {
        if (_keyInterceptor != null)
        {
            await _keyInterceptor.DisposeAsync();
            _keyInterceptor.AttackKeyDown -= OnAttackKeyDown;
            _keyInterceptor.MoveKeyDown -= OnMoveKeyDown;
        }

        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        _boxSize = 64;
        _wallWidth = Math.Max(1, _boxSize / 10);
    }

    protected override void OnParametersSet()
    {
        _originalSize = MazeSize ?? 16;
        _density = MazeDensity ?? 40;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GenerateAsync();

            if (_keyInterceptor != null)
            {
                _keyInterceptor.AttackKeyDown += OnAttackKeyDown;
                _keyInterceptor.MoveKeyDown += OnMoveKeyDown;
            }
        }
    }

    private async void OnRunnerMoved(object? sender, Position args)
    {
        _vision.SetPosition(_labyrinth.Runner.Position);

        // TODO подумать как вынести строку
        await SoundService.PlayAsync("step");

        await ForceRender();
        StateHasChanged();
    }

    private void OnExitFound(object? sender, EventArgs args)
    {
        _isExitFound = true;
    }

    private async void OnItemPickedUp(object? sender, TileFeature args)
    {
        await SoundService.PlayAsync(args.PickUpSound);
    }

    private void OnMoveKeyDown(object? sender, MoveEventArgs args)
    {
        if (_isExitFound)
        {
            return;
        }

        _labyrinth.Move(args.Direction);
    }

    private void OnAttackKeyDown(object? sender, AttackEventArgs args)
    {
        if (_isExitFound)
        {
            return;
        }

        Item? item = args.Item;

        if (item != null)
        {
            _labyrinth.Runner.UseItem(item, args.Direction);
        }
    }

    private async void OnItemUsed(object? sender, Item item)
    {
        await SoundService.PlayAsync(item.SoundSettings?.UseSound);

        await ForceRender();
        StateHasChanged();
    }

    private async Task GenerateAsync()
    {
        AnimationService.StartRandomAnimationEffect();
        // todo Костыль чтоб цвет обновлялся, надо больше времени подумать.
        // (Не перерисовывает если стена осталась на прежнем месте)
        // Но в принципе то работает)))))) 
        await Task.Delay(1);

        _seeder.Reload();

        if (_labyrinth != null)
        {
            _labyrinth.RunnerMoved -= OnRunnerMoved;
            _labyrinth.ExitFound -= OnExitFound;
            _labyrinth.ItemPickedUp -= OnItemPickedUp;
            _labyrinth.Runner.Inventory.ItemUsed -= OnItemUsed;
        }

        _isExitFound = false;

        _originalSize = Math.Max(MinSize, Math.Min(MaxSize, _originalSize));

        _labyrinth = new Labyrinth(_seeder);
        _labyrinth.Init(_originalSize, _originalSize, _density);

        _labyrinth.RunnerMoved += OnRunnerMoved;
        _labyrinth.ExitFound += OnExitFound;
        _labyrinth.ItemPickedUp += OnItemPickedUp;
        _labyrinth.Runner.Inventory.ItemUsed += OnItemUsed;

        _keyInterceptor?.InitializeItems();

        _vision = new Vision(_originalSize, _originalSize);
        _vision.SetPosition(_labyrinth.Runner.Position);

        _renderParameter = new MazeRenderParameters(_labyrinth, _boxSize, _wallWidth, _vision);

        StateHasChanged();

        await ForceRender();

        _isInit = true;
        StateHasChanged();
    }

    private async Task ForceRender()
    {
        await (_mazeFloor?.ForceRender() ?? Task.CompletedTask);
        await (_mazeWalls?.ForceRender() ?? Task.CompletedTask);
        await (_mazeEntities?.ForceRender() ?? Task.CompletedTask);
    }
}
