namespace Management.Endpoints.ServiceConfigs.Update;

public class UpdateServiceConfigRequest
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Secret { get; set; }

    public string BaseUrl { get; set; }
}