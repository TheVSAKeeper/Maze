namespace Labirint.Web.Services.Dialogs;

public sealed class DialogInstance
{
    private readonly TaskCompletionSource<DialogResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly DialogService _service;

    internal DialogInstance(DialogService service, Type contentType, string title, DialogParameters parameters, DialogOptions options)
    {
        _service = service;
        ContentType = contentType;
        Title = title;
        Parameters = parameters;
        Options = options;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public Type ContentType { get; }

    public string Title { get; }

    public DialogParameters Parameters { get; }

    public DialogOptions Options { get; }

    public Task<DialogResult> Result => _completion.Task;

    public void Close(object? data = null)
    {
        Complete(DialogResult.Ok(data));
    }

    public void Cancel()
    {
        Complete(DialogResult.Cancelled());
    }

    private void Complete(DialogResult result)
    {
        if (_completion.TrySetResult(result))
        {
            _service.Remove(this);
        }
    }
}
