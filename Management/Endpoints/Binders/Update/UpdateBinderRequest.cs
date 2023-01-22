namespace Management.Endpoints.Binders.Update;

public class UpdateBinderRequest
{
    public string Id { get; set; }

    public string ApiUrl { get; set; }
    
    public Dictionary<string,string> Metas { get; set; }
}