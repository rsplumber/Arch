using FastEndpoints;

namespace Arch.Endpoints.Post;

public class PostGroup : Group
{
    public const string Name = "Post";

    public PostGroup()
    {
        Configure(Name,definition => {definition.Tags(Name);});
    }
}