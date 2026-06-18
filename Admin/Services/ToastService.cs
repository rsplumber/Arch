namespace Arch.Admin.Services;

public enum ToastKind { Success, Error }

public sealed record Toast(Guid Id, ToastKind Kind, string Message);

/// <summary>Tiny per-circuit pub/sub for transient notifications.</summary>
public sealed class ToastService
{
    private readonly List<Toast> _toasts = [];
    public IReadOnlyList<Toast> Toasts => _toasts;

    public event Action? OnChanged;

    public void Success(string message) => Add(ToastKind.Success, message);
    public void Error(string message) => Add(ToastKind.Error, message);

    private void Add(ToastKind kind, string message)
    {
        _toasts.Add(new Toast(Guid.NewGuid(), kind, message));
        OnChanged?.Invoke();
    }

    public void Remove(Guid id)
    {
        _toasts.RemoveAll(t => t.Id == id);
        OnChanged?.Invoke();
    }
}
