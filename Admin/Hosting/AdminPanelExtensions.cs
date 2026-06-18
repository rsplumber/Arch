using System.Security.Claims;
using Arch.Admin.Components;
using Microsoft.AspNetCore.Authentication;
using Arch.Admin.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Admin.Hosting;

/// <summary>
/// Registration + hosting helpers for the embedded Arch admin panel.
///
/// The panel is hosted in its own isolated pipeline branch via <see cref="MapArchAdmin"/>.
/// This matters: the Arch gateway's request-extractor middleware returns 404 for any path
/// that is not a registered upstream endpoint, and it runs before routing. Branching with
/// <c>app.Map(basePath, ...)</c> means admin requests (including the Blazor SignalR hub and
/// framework assets) never enter the gateway pipeline at all.
/// </summary>
public static class AdminPanelExtensions
{
    /// <summary>Registers admin services, cookie session auth, and Razor (Blazor Server) components.</summary>
    public static IServiceCollection AddArchAdmin(this IServiceCollection services, Action<AdminPanelOptions>? configure = null)
    {
        var options = new AdminPanelOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddScoped<AdminConfigService>();
        services.AddScoped<ToastService>();

        services.AddAuthentication(AdminAuthDefaults.Scheme)
            .AddCookie(AdminAuthDefaults.Scheme, cookie =>
            {
                cookie.Cookie.Name = AdminAuthDefaults.CookieName;
                cookie.Cookie.HttpOnly = true;
                cookie.Cookie.SameSite = SameSiteMode.Lax;
                // Paths are relative to the branch PathBase (e.g. /admin), which the cookie
                // middleware re-prepends — so these must NOT include the base path again.
                cookie.LoginPath = "/login";
                cookie.LogoutPath = "/logout";
                cookie.AccessDeniedPath = "/login";
                cookie.ExpireTimeSpan = options.SessionTimeout;
                cookie.SlidingExpiration = true;
            });
        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        return services;
    }

    /// <summary>Mounts the admin panel under its configured base path as an isolated branch.</summary>
    public static WebApplication MapArchAdmin(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<AdminPanelOptions>();

        app.Map(options.BasePath, branch =>
        {
            branch.UseRouting();
            branch.UseAuthentication();
            branch.UseAuthorization();
            branch.UseAntiforgery();

            branch.UseEndpoints(endpoints =>
            {
                // Serves Blazor framework assets (blazor.web.js, etc.) as fingerprinted
                // endpoints. PathBase is the configured base path, so these resolve under
                // e.g. /admin/_framework/*.
                endpoints.MapStaticAssets();

                endpoints.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode();

                MapAuthEndpoints(endpoints, options);
            });
        });

        return app;
    }

    private static void MapAuthEndpoints(IEndpointRouteBuilder endpoints, AdminPanelOptions options)
    {
        // Login/logout run as plain form POSTs so the auth cookie is written on a real HTTP
        // response (it cannot be set from inside an interactive Blazor circuit). They live under
        // /auth/* to avoid colliding with the routable "/login" Blazor page.
        endpoints.MapPost("/auth/login", async (HttpContext http) =>
        {
            var form = await http.Request.ReadFormAsync();
            var username = form["username"].ToString();
            var password = form["password"].ToString();
            var returnUrl = form["returnUrl"].ToString();

            var ok = CryptographicEquals(username, options.Username) && CryptographicEquals(password, options.Password);
            if (!ok)
            {
                return Results.LocalRedirect($"{options.BasePath}/login?error=1");
            }

            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, options.Username)],
                AdminAuthDefaults.Scheme);
            await http.SignInAsync(AdminAuthDefaults.Scheme, new ClaimsPrincipal(identity));

            var target = !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith(options.BasePath, StringComparison.Ordinal)
                ? returnUrl
                : options.BasePath;
            return Results.LocalRedirect(target);
        }).DisableAntiforgery();

        endpoints.MapPost("/auth/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(AdminAuthDefaults.Scheme);
            return Results.LocalRedirect($"{options.BasePath}/login");
        }).DisableAntiforgery();
    }

    /// <summary>Length-aware, fixed-time string comparison to avoid leaking length/timing.</summary>
    private static bool CryptographicEquals(string a, string b)
    {
        var left = System.Text.Encoding.UTF8.GetBytes(a);
        var right = System.Text.Encoding.UTF8.GetBytes(b);
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(left, right);
    }
}
