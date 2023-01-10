namespace Core.ServiceConfigs.Types;

public sealed class ServiceStatus : Enumeration
{
    public static readonly ServiceStatus Enable = new(1, nameof(Enable));
    public static readonly ServiceStatus Disable = new(2, nameof(Disable));

    private ServiceStatus(int id, string name) : base(id, name)
    {
    }
}