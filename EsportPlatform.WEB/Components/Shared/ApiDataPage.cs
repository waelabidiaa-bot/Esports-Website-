using EsportPlatform.WEB.Services;
using Microsoft.AspNetCore.Components;

namespace EsportPlatform.WEB.Components.Shared;

/// <summary>
/// Base class for pages backed by <see cref="ApiDataService"/>.
///
/// Why pages must load their own data: when a Blazor Server circuit starts from a
/// prerendered page, .NET 10 resumes the prerendered app shell (layouts and any
/// components above the routed page are restored from serialized state and their
/// lifecycle does not run again), while the routed page itself is re-created inside
/// the circuit's DI scope. Initialization placed in a layout component therefore
/// never runs for the circuit, and the page would render with an empty service.
///
/// Each page calling <see cref="ApiDataService.InitializeAsync"/> is single-flight,
/// so concurrent pages in the same scope share one request fan-out.
/// </summary>
public abstract class ApiDataPage : ComponentBase, IDisposable
{
    [Inject]
    protected ApiDataService Data { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Data.ApiConnectionChanged += OnApiConnectionChanged;

        await Data.InitializeAsync();

        CheckConnection();
    }

    private void OnApiConnectionChanged()
    {
        // Runs on the renderer's sync context, then re-renders the page so
        // freshly loaded data appears without further navigation.
        _ = InvokeAsync(() =>
        {
            CheckConnection();
            StateHasChanged();
        });
    }

    private void CheckConnection()
    {
        var currentPath = Navigation.ToBaseRelativePath(Navigation.Uri).Trim('/');

        if (!Data.IsApiAvailable &&
            !currentPath.Equals("server-error", StringComparison.OrdinalIgnoreCase))
        {
            Navigation.NavigateTo("/server-error", forceLoad: false);
        }
    }

    public virtual void Dispose()
    {
        Data.ApiConnectionChanged -= OnApiConnectionChanged;
    }
}
