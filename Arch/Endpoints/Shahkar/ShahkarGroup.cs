using FastEndpoints;

namespace Arch.Endpoints.Shahkar;

public class ShahkarGroup : Group
{
    public const string Name = "Shahkar";

    public ShahkarGroup()
    {
        Configure(Name, definition => { definition.Tags(Name); });
    }
}