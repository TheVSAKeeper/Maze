using Labirint.Core.TileFeatures.Base;
using Labirint.Web.Common.Animation;
using Labirint.Web.Common.Ui;
using Labirint.Web.Components;
using Labirint.Web.Components.Dialogs;
using Labirint.Web.Components.Ui;
using Labirint.Web.Parameters;
using Labirint.Web.Services.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Labirint.Web.Pages;

public partial class Maze : IAsyncDisposable
{
    private const int MinSize = 1;
    private const int MaxSize = 500;

    private const int MinDensity = 0;
    private const int MaxDensity = 100;

    private const int DefaultSize = 16;
    private const int DefaultDensity = 40;

    private bool _isExitFound;
    private bool _isContinueGame;
    private bool _isInit;
    private bool _isSettingsOpen;
    private bool _isSettingsTrapped;
    private bool _isRegenerationPending;
    private bool _isGenerating;

    private string? _appliedSeed;
    private int? _appliedSize;
    private int? _appliedDensity;

    private ElementReference _settingsSheet;

    private int _originalSize;
    private int _density;

    private int _boxSize;
    private int _wallWidth;

    private int _moveCount;
    private int _displayScore;

    private int _runnerScaleX = 1;

    private MazeFloor? _mazeFloor;
    private MazeWalls? _mazeWalls;
    private MazeEntities? _mazeEntities;
    private MazeRenderParameters? _renderParameter;
    private KeyInterceptor? _keyInterceptor;
    private RunnerInventory? _runnerInventory;

    private Labyrinth _labyrinth = null!;
    private RandomGenerator _seeder = null!;
    private Vision _vision = null!;
    private TouchInterceptor? _touchInterceptor;

    [Parameter]
    public string? Seed { get; set; }

    [SupplyParameterFromQuery(Name = RandomGenerator.SizeQueryName)]
    public int? MazeSize { get; set; }

    [SupplyParameterFromQuery(Name = RandomGenerator.DensityQueryName)]
    public int? MazeDensity { get; set; }

    [Inject]
    private SoundService SoundService { get; set; } = null!;

    [Inject]
    private AnimationService AnimationService { get; set; } = null!;

    [Inject]
    private DialogService DialogService { get; set; } = null!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    [Inject]
    private MotionService MotionService { get; set; } = null!;

    // Проверка на null и инициализацию (дополнительная проверка, если флаг выставили в true, а значение у не null полей не выставили)
    private bool IsInit => _isInit && _labyrinth != null && _seeder != null && _vision != null && _renderParameter != null;

    private bool IsBusy => IsInit == false || _isGenerating;

    public ValueTask DisposeAsync()
    {
        GlobalParameters.LabyrinthChanged -= OnLabyrinthParametersChanged;

        if (_keyInterceptor != null)
        {
            _keyInterceptor.AttackKeyDown -= OnAttackKeyDown;
            _keyInterceptor.MoveKeyDown -= OnMoveKeyDown;
        }

        if (_labyrinth != null)
        {
            _labyrinth.RunnerMoved -= OnRunnerMoved;
            _labyrinth.ExitFound -= OnExitFound;
            _labyrinth.ItemPickedUp -= OnItemPickedUp;
            _labyrinth.Runner.Inventory.ItemUsed -= OnItemUsed;
            _labyrinth.Runner.Inventory.ScoreIncreased -= OnScoreIncreased;
        }

        if (_touchInterceptor != null)
        {
            _touchInterceptor.Moved -= OnMoved;
        }

        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    protected override void OnInitialized()
    {
        _boxSize = 64;
        _wallWidth = Math.Max(1, _boxSize / 10);

        GlobalParameters.LabyrinthChanged += OnLabyrinthParametersChanged;
    }

    protected override void OnParametersSet()
    {
        var isRouteChanged = Seed != _appliedSeed || MazeSize != _appliedSize || MazeDensity != _appliedDensity;

        _appliedSeed = Seed;
        _appliedSize = MazeSize;
        _appliedDensity = MazeDensity;

        _originalSize = MazeSize ?? DefaultSize;
        _density = MazeDensity ?? DefaultDensity;

        if (isRouteChanged && _isInit)
        {
            _isRegenerationPending = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender == false)
        {
            if (_isSettingsOpen != _isSettingsTrapped)
            {
                _isSettingsTrapped = _isSettingsOpen;

                await (_isSettingsOpen
                    ? JSRuntime.InvokeVoidAsync("labirintDialog.trap", _settingsSheet)
                    : JSRuntime.InvokeVoidAsync("labirintDialog.release"));
            }

            if (_isRegenerationPending)
            {
                _isRegenerationPending = false;
                await GenerateAsync();
            }

            return;
        }

        _labyrinth = new(_seeder);
        _labyrinth.RunnerMoved += OnRunnerMoved;
        _labyrinth.ExitFound += OnExitFound;
        _labyrinth.ItemPickedUp += OnItemPickedUp;
        _labyrinth.Runner.Inventory.ItemUsed += OnItemUsed;
        _labyrinth.Runner.Inventory.ScoreIncreased += OnScoreIncreased;

        if (_keyInterceptor != null)
        {
            _keyInterceptor.AttackKeyDown += OnAttackKeyDown;
            _keyInterceptor.MoveKeyDown += OnMoveKeyDown;
            _keyInterceptor.InitializeItems();
        }

        await GenerateAsync();

        if (_touchInterceptor != null)
        {
            _touchInterceptor.Moved += OnMoved;
        }
    }

    private void OpenSettings()
    {
        _isSettingsOpen = true;
    }

    private void CloseSettings()
    {
        if (_isSettingsOpen == false)
        {
            return;
        }

        _isSettingsOpen = false;
    }

    private void OnSettingsKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            CloseSettings();
        }
    }

    private void OnMoved(object? sender, Direction args)
    {
        _keyInterceptor?.OnKeyDown(args);
    }

    private void OnLabyrinthParametersChanged(object? sender, EventArgs args)
    {
        if (IsInit == false)
        {
            return;
        }

        RunSafe(ForceRender);
    }

    private void OnRunnerMoved(object? sender, Position args)
    {
        _moveCount++;
        _vision.SetPosition(_labyrinth.Runner.Position);

        RunSafe(async () =>
        {
            await ForceRender();

            // TODO подумать как вынести строку
            SoundService.Play("step");
        });
    }

    private void OnExitFound(object? sender, EventArgs args)
    {
        if (_isContinueGame)
        {
            return;
        }

        _isExitFound = true;

        RunSafe(ShowWinDialogAsync);
    }

    private async Task ShowWinDialogAsync()
    {
        DialogParameters parameters = new()
        {
            [nameof(WinDialog.OnRestart)] = (Func<Task>)GenerateAsync,
            [nameof(WinDialog.OnRepeat)] = (Func<Task>)RepeatAsync,
            [nameof(WinDialog.Seeder)] = _seeder,
            [nameof(WinDialog.Score)] = _labyrinth.Runner.Score,
            [nameof(WinDialog.MoveCount)] = _moveCount,
            [nameof(WinDialog.Size)] = _originalSize,
        };

        var result = await DialogService.ShowAsync<WinDialog>("Финал Лабиринта", parameters, new DialogOptions
        {
            CloseButton = false,
            CloseOnBackdropClick = false,
            CloseOnEscape = false,
            Width = DialogWidth.Large,
        });

        _isContinueGame = result.GetValue<bool>();

        if (_isContinueGame)
        {
            _isExitFound = false;
        }
    }

    private void OnItemPickedUp(object? sender, TileFeature args)
    {
        RunSafe(async () =>
        {
            await ForceRender();

            SoundService.Play(args.PickUpSound);
        });
    }

    private void OnScoreIncreased(object? sender, int amount)
    {
        RunSafe(async () =>
        {
            await Task.Delay(AnimatedStackExtensions.PickupFlightDuration);

            _displayScore = _labyrinth.Runner.Score;
            StateHasChanged();
        });
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

        var item = args.Item;

        if (item == null)
        {
            return;
        }

        if (_labyrinth.Runner.Inventory.CanUse(item) == false || _runnerInventory == null)
        {
            _labyrinth.Runner.UseItem(item, args.Direction);
            return;
        }

        if (_runnerInventory.TryStartCast(item) == false)
        {
            return;
        }

        var direction = args.Direction;
        var flightDuration = MotionService.IsReduced ? 0 : AnimatedStackExtensions.UseFlightDuration;

        RunSafe(async () =>
        {
            await Task.Delay(flightDuration);

            if (_isExitFound)
            {
                _runnerInventory.CancelCast(item);
                return;
            }

            _labyrinth.Runner.UseItem(item, direction);
        });
    }

    private void OnItemUsed(object? sender, Item item)
    {
        RunSafe(async () =>
        {
            await ForceRender();
            SoundService.Play(item.SoundSettings?.UseSound);
        });
    }

    private Task GenerateAsync()
    {
        return GenerateAsync(false);
    }

    private Task RepeatAsync()
    {
        return GenerateAsync(true);
    }

    private async Task GenerateAsync(bool isSeedRepeated)
    {
        AnimationService.StartRandomAnimationEffect();

        _isGenerating = true;
        StateHasChanged();
        await Task.Delay(1);

        if (isSeedRepeated)
        {
            _seeder.Repeat();
        }
        else
        {
            _seeder.Reload();
        }

        _isExitFound = false;
        _isContinueGame = false;
        _moveCount = 0;
        _displayScore = 0;

        _originalSize = Math.Max(MinSize, Math.Min(MaxSize, _originalSize));
        _density = Math.Max(MinDensity, Math.Min(MaxDensity, _density));

        _labyrinth.Init(_originalSize, _originalSize, _density);

        _vision = new(_originalSize, _originalSize);
        _vision.SetPosition(_labyrinth.Runner.Position);

        _renderParameter = new(_labyrinth, _boxSize, _wallWidth, _vision);
        _isGenerating = false;

        StateHasChanged();

        await ForceRender();

        _isInit = true;
        StateHasChanged();
    }

    private async Task ForceRender()
    {
        await Task.WhenAll(_mazeFloor?.ForceRenderAsync() ?? Task.CompletedTask,
            _mazeWalls?.ForceRenderAsync() ?? Task.CompletedTask,
            _mazeEntities?.ForceRenderAsync() ?? Task.CompletedTask);

        StateHasChanged();
    }
}
