//-----------------------------------------------------------------------
// <copyright file="NinetiesThemeDecorations.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Nineties decorations code-behind
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Shared;

/// <summary>
/// Controls 90s-themed decorative elements and hit counter state.
/// </summary>
public partial class NinetiesThemeDecorations : ComponentBase, IDisposable
{
    private bool isNinetiesTheme = false;
    private int hitCount = 0;
    private const string HitCountKey = "nineties-hit-count";
    private const string ThemeKey = "theme-mode";

    /// <summary>
    /// Subscribes to theme change notifications.
    /// </summary>
    protected override void OnInitialized()
    {
        ThemeService.OnThemeChanged += HandleThemeChanged;
    }

    /// <summary>
    /// Initializes theme and counter state after first render.
    /// </summary>
    /// <param name="firstRender">Whether this is the first render pass.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var theme = await JS.InvokeAsync<string>("localStorage.getItem", ThemeKey);

            // Validate theme value
            var validModes = new[] { "light", "dark", "nineties", "system" };
            if (!string.IsNullOrEmpty(theme) && !validModes.Contains(theme))
            {
                Console.WriteLine($"Warning: Invalid theme mode '{theme}' in localStorage, clearing.");
                await JS.InvokeVoidAsync("localStorage.removeItem", ThemeKey);
                theme = null;
            }

            isNinetiesTheme = theme == "nineties";

            if (isNinetiesTheme)
            {
                await IncrementHitCounter();
                StateHasChanged();
            }
        }
    }

    private async void HandleThemeChanged()
    {
        await UpdateThemeStatus();
    }

    /// <summary>
    /// Refreshes theme state and updates the hit counter when needed.
    /// </summary>
    /// <returns>A task that completes when the state update is finished.</returns>
    public async Task UpdateThemeStatus()
    {
        var theme = await JS.InvokeAsync<string>("localStorage.getItem", ThemeKey);

        // Validate theme value
        var validModes = new[] { "light", "dark", "nineties", "system" };
        if (!string.IsNullOrEmpty(theme) && !validModes.Contains(theme))
        {
            Console.WriteLine($"Warning: Invalid theme mode '{theme}' in localStorage, clearing.");
            await JS.InvokeVoidAsync("localStorage.removeItem", ThemeKey);
            theme = null;
        }

        var wasNineties = isNinetiesTheme;
        isNinetiesTheme = theme == "nineties";

        if (isNinetiesTheme && !wasNineties)
        {
            await IncrementHitCounter();
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Increments and persists the local 90s hit counter.
    /// </summary>
    /// <returns>A task that completes when persistence finishes.</returns>
    private async Task IncrementHitCounter()
    {
        var countStr = await JS.InvokeAsync<string>("localStorage.getItem", HitCountKey);

        // Validate hit counter value
        if (int.TryParse(countStr, out var count))
        {
            hitCount = count + 1;
        }
        else if (!string.IsNullOrEmpty(countStr))
        {
            // Invalid hit counter value - clear it and start fresh
            Console.WriteLine($"Warning: Invalid hit-counter value '{countStr}' in localStorage, clearing.");
            await JS.InvokeVoidAsync("localStorage.removeItem", HitCountKey);
            hitCount = 1337;
        }
        else
        {
            hitCount = 1337;
        }

        await JS.InvokeVoidAsync("localStorage.setItem", HitCountKey, hitCount.ToString());
    }

    /// <summary>
    /// Unsubscribes from theme notifications.
    /// </summary>
    public void Dispose()
    {
        ThemeService.OnThemeChanged -= HandleThemeChanged;
    }
}