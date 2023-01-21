namespace Core.ServiceConfigs;

public class ServiceConfig
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Secret { get; set; }

    public string BaseUrl { get; set; }

    public List<Binder> Binders { get; set; }
}