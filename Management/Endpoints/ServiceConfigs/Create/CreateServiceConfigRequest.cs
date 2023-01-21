namespace Management.Endpoints.ServiceConfigs.Create;

public class CreateServiceConfigRequest
{
    public string Name { get; set; }

    public string Secret { get; set; }

    public string BaseUrl { get; set; }
}