namespace Arch.Admin.Hosting;

/// <summary>Configuration for the embedded Arch admin panel.</summary>
public sealed class AdminPanelOptions
{
    /// <summary>Path the panel is mounted under. Defaults to <c>/admin</c>.</summary>
    public string BasePath { get; set; } = "/admin";

    /// <summary>Admin username. Defaults to <c>admin</c>.</summary>
    public string Username { get; set; } = "admin";

    /// <summary>Admin password. Defaults to <c>admin</c>.</summary>
    public string Password { get; set; } = "admin";

    /// <summary>How long an admin session stays valid. Defaults to 8 hours.</summary>
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromHours(8);
}

/// <summary>Shared constants for the admin authentication scheme.</summary>
public static class AdminAuthDefaults
{
    public const string Scheme = "ArchAdminCookie";
    public const string CookieName = "arch_admin_session";
}
