namespace Arch;

public class TesServicesOptions
{
    public List<Service> Services { get; set; } = new();
}

public class Service
{
    public string Name { get; set; } = default!;

    public string BaseUrl { get; set; } = default!;

    public string Secret { get; set; } = default!;
}