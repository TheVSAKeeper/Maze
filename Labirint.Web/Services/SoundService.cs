using Labirint.Web.Parameters;

namespace Labirint.Web.Services;

public class SoundService(IJSRuntime jsRuntime)
{
    public void Play(string? soundType)
    {
        if (string.IsNullOrWhiteSpace(soundType) || GlobalParameters.Labyrinth.IsSoundOn == false)
        {
            return;
        }

        try
        {
            ((IJSInProcessRuntime)jsRuntime).InvokeVoid("playSound", soundType, GlobalParameters.Labyrinth.SoundVolume);
        }
        catch (JSException)
        {
        }
    }
}
