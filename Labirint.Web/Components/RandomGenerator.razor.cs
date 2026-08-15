using Labirint.Web.Common.Seeding;
using Labirint.Web.Components.Dialogs;
using Labirint.Web.Services.Dialogs;
using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Components;

public partial class RandomGenerator
{
    public const string SizeQueryName = "s";
    public const string DensityQueryName = "d";
    private const string MazePageUrl = "labirint";

    private readonly SeedSource _seed = new();

    private string? _appliedSeed;

    public SeedSource Source => _seed;

    public string Link => GetShareLink();

    [Parameter]
    public string? Seed { get; set; }

    [Parameter]
    public int Size { get; set; }

    [Parameter]
    public int Density { get; set; }

    [Parameter]
    public bool IsExitFound { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private DialogService DialogService { get; set; } = null!;

    private bool IsGenerateRequired => _seed.IsGenerateRequired;

    public void Repeat()
    {
        _seed.Repeat();
        StateHasChanged();
    }

    public void Reload(bool force = false)
    {
        _seed.Reload(force);
        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        if (Seed == _appliedSeed)
        {
            return;
        }

        _appliedSeed = Seed;
        _seed.UserSeed = string.IsNullOrWhiteSpace(Seed) ? null : Seed;
    }

    private void ResetSeed()
    {
        _seed.ResetSeed();
        StateHasChanged();
    }

    private string GetShareLink()
    {
        var linkWithSeed = $"{NavigationManager.BaseUri}{MazePageUrl}/{_seed.CurrentSeed}";

        return NavigationManager.GetUriWithQueryParameters(linkWithSeed, new Dictionary<string, object?>
        {
            [SizeQueryName] = Size,
            [DensityQueryName] = Density,
        });
    }

    private async Task ShowShareDialogAsync()
    {
        await DialogService.ShowAsync<ShareDialog>("Поделиться лабиринтом", new DialogParameters
        {
            [nameof(ShareDialog.Link)] = Link,
            [nameof(ShareDialog.UserSeed)] = _seed.UserSeed,
            [nameof(ShareDialog.IsExitFound)] = IsExitFound,
        }, new DialogOptions { Width = DialogWidth.Medium });
    }
}
