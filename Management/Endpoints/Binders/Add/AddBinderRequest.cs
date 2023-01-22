namespace Management.Endpoints.Binders.Add;

public class AddBinderRequest
{
    public Guid ServiceConfigId { get; set; }

    public string ApiUrl { get; set; }

    public string Bind { get; set; }

    public Dictionary<string, string> Metas { get; set; }
}