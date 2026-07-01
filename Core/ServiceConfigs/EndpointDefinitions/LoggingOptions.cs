namespace Arch.Core.ServiceConfigs.EndpointDefinitions;

public sealed record LoggingOptions
{
    private const string DisabledValue = "disable";
    private const string LoggingJustErrorsMetaValue = "error";
    private const string LoggingInformalMetaValue = "informal";

    public LoggingOptions(string? loggingMeta)
    {
        Disabled = loggingMeta switch
        {
            DisabledValue => true,
            _ => false
        };

        JustError = loggingMeta == LoggingJustErrorsMetaValue;
        Informal = loggingMeta == LoggingInformalMetaValue;
    }

    public bool Disabled { get; private set; }

    public bool JustError { get; private set; }

    public bool Informal { get; private set; }
}
