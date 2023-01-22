namespace Core.ServiceConfigs;

public class Binder
{
    public Guid Id { get; set; }

    public string Bind { get; set; }
    public string ApiUrl { get; set; }

    public List<Meta> Meta { get; set; } = new();

    public ServiceConfig ServiceConfig { get; set; }
}