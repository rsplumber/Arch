namespace Core.Binders.Types;

public sealed class BinderStatus : Enumeration
{
    public static readonly BinderStatus Enable = new(1, nameof(Enable));
    public static readonly BinderStatus Disable = new(2, nameof(Disable));
    
    private BinderStatus(int id, string name) : base(id, name)
    {
    }
}