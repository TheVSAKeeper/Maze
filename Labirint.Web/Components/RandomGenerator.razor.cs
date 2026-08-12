using Labirint.Core.Interfaces;
using Labirint.Web.Components.Dialogs;
using Labirint.Web.Services.Dialogs;
using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;
using System.Text;

namespace Labirint.Web.Components;

public partial class RandomGenerator : IRandom
{
    public const string SizeQueryName = "s";
    public const string DensityQueryName = "d";
    private const string MazePageUrl = "labirint";

    private int _currentSeed;

    private Random? _random;
    private string? _userSeed;
    private string? _appliedSeed;

    public Random Generator => _random ?? Random.Shared;

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

    private bool IsGenerateRequired => _currentSeed < 0;

    public void Reload(bool force = false)
    {
        if (force || string.IsNullOrWhiteSpace(_userSeed))
        {
            ReloadWithRandomSeed();
            return;
        }

        _currentSeed = _userSeed.All(char.IsDigit) && int.TryParse(_userSeed, out var parsedSeed)
            ? parsedSeed
            : GenerateSeed(_userSeed);

        _random = new(_currentSeed);
        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        if (Seed == _appliedSeed)
        {
            return;
        }

        _appliedSeed = Seed;
        _userSeed = string.IsNullOrWhiteSpace(Seed) ? null : Seed;
    }

    private static int GenerateSeed(string input)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var result = BitConverter.ToInt32(hashBytes, 0);
        return Math.Abs(result);
    }

    private void ReloadWithRandomSeed()
    {
        _userSeed = null;
        _currentSeed = Random.Shared.Next();
        _random = new(_currentSeed);
        StateHasChanged();
    }

    private void ResetSeed()
    {
        _currentSeed = -1;
        StateHasChanged();
    }

    private string GetShareLink()
    {
        var linkWithSeed = $"{NavigationManager.BaseUri}{MazePageUrl}/{_currentSeed}";

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
            [nameof(ShareDialog.UserSeed)] = _userSeed,
            [nameof(ShareDialog.IsExitFound)] = IsExitFound,
        }, new DialogOptions { Width = DialogWidth.Medium });
    }
}
