using FastEndpoints;

namespace Arch.Endpoints.SabtAhval;

public class SabtAhvalGroup : Group
{
    public const string Name = "SabtAhval";

    public SabtAhvalGroup()
    {
        Configure(Name,definition => {definition.Tags(Name);});
    }
}