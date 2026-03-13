//-----------------------------------------------------------------------
// <copyright file="ThemeSwitcher.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Theme switcher component code-behind
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Shared;

/// <summary>
/// Applies and persists user-selected UI theme.
/// </summary>
public partial class ThemeSwitcher : ComponentBase
{
    private enum ThemeMode
    {
        Light,
        Dark,
        Nineties,
        System,
    }

    private const string ThemeKey = "theme-mode";

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private DadABase.Web.Repositories.ThemeService ThemeService { get; set; } = default!;
    private bool _initialized = false;
    private string _pendingTheme = null;

    /// <summary>
    /// Applies persisted theme during first render and deferred updates thereafter.
    /// </summary>
    /// <param name="firstRender">Whether this is the first render pass.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initialized)
        {
            _initialized = true;
            var mode = await JS.InvokeAsync<string>("localStorage.getItem", ThemeKey);
            if (!string.IsNullOrEmpty(mode))
            {
                await ApplyTheme(mode);
            }
            else
            {
                await ApplyTheme("system");
            }
        }
        else if (_pendingTheme is not null)
        {
            await ApplyTheme(_pendingTheme);
            _pendingTheme = null;
        }
    }

    /// <summary>
    /// Persists a selected theme and schedules application to the page.
    /// </summary>
    /// <param name="mode">The target theme mode.</param>
    /// <returns>A task that completes when persistence is done.</returns>
    private async Task SetTheme(ThemeMode mode)
    {
        var modeStr = mode.ToString().ToLower();
        await JS.InvokeVoidAsync("localStorage.setItem", ThemeKey, modeStr);
        _pendingTheme = modeStr;
        StateHasChanged();
    }

    /// <summary>
    /// Applies CSS/body attributes for the selected theme and notifies listeners.
    /// </summary>
    /// <param name="mode">The theme mode text from storage.</param>
    /// <returns>A task that completes when theme application is done.</returns>
    private async Task ApplyTheme(string mode)
    {
        // Remove all theme classes.
        await JS.InvokeVoidAsync("eval", "document.body.classList.remove('theme-light','theme-dark','theme-nineties','theme-system');");
        if (mode == "light")
        {
            await JS.InvokeVoidAsync("eval", "document.body.classList.add('theme-light'); document.body.setAttribute('data-bs-theme','light');");
        }
        else if (mode == "dark")
        {
            await JS.InvokeVoidAsync("eval", "document.body.classList.add('theme-dark'); document.body.setAttribute('data-bs-theme','dark');");
        }
        else if (mode == "nineties")
        {
            await JS.InvokeVoidAsync("eval", "document.body.classList.add('theme-nineties'); document.body.setAttribute('data-bs-theme','light');");
        }
        else
        {
            await JS.InvokeVoidAsync("eval", "document.body.classList.add('theme-system'); document.body.removeAttribute('data-bs-theme');");
        }

        ThemeService.NotifyThemeChanged();
    }
}