using System.Globalization;
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

    public string RouteSeed => string.IsNullOrWhiteSpace(_seed.UserSeed) == false
                               && _seed.UserSeed.All(symbol => symbol == '.') == false
                               && SeedSource.ParseSeed(_seed.UserSeed) == _seed.CurrentSeed
        ? _seed.UserSeed
        : _seed.CurrentSeed.ToString(CultureInfo.InvariantCulture);

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

    public static int? ParseQueryNumber(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : null;
    }

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

    public void ReplaceRoute(string seed, int size, int density)
    {
        _appliedSeed = seed;
        NavigationManager.NavigateTo(BuildLink(seed, size, density), replace: true);
    }

    private string GetShareLink()
    {
        return BuildLink(_seed.CurrentSeed.ToString(CultureInfo.InvariantCulture), Size, Density);
    }

    private string BuildLink(string seed, int size, int density)
    {
        var linkWithSeed = $"{NavigationManager.BaseUri}{MazePageUrl}/{Uri.EscapeDataString(seed)}";

        return NavigationManager.GetUriWithQueryParameters(linkWithSeed, new Dictionary<string, object?>
        {
            [SizeQueryName] = size,
            [DensityQueryName] = density,
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
