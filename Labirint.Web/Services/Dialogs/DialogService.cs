using Microsoft.AspNetCore.Components;

namespace Labirint.Web.Services.Dialogs;

public sealed class DialogService
{
    private readonly List<DialogInstance> _instances = [];

    public event Action? Changed;

    public IReadOnlyList<DialogInstance> Instances => [.. _instances];

    public Task<DialogResult> ShowAsync<TDialog>(string title, DialogParameters? parameters = null, DialogOptions? options = null)
        where TDialog : IComponent
    {
        DialogInstance instance = new(this, typeof(TDialog), title, parameters ?? [], options ?? new DialogOptions());

        _instances.Add(instance);
        Changed?.Invoke();

        return instance.Result;
    }

    internal void Remove(DialogInstance instance)
    {
        if (_instances.Remove(instance))
        {
            Changed?.Invoke();
        }
    }
}
