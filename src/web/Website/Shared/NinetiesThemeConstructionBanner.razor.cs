//-----------------------------------------------------------------------
// <copyright file="NinetiesThemeConstructionBanner.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Nineties construction banner code-behind
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Shared;

/// <summary>
/// Controls visibility of the 90s under-construction banner.
/// </summary>
public partial class NinetiesThemeConstructionBanner : ComponentBase, IDisposable
{
    private bool isNinetiesTheme = false;
    private const string ThemeKey = "theme-mode";

    /// <summary>
    /// Subscribes to theme change notifications.
    /// </summary>
    protected override void OnInitialized()
    {
        ThemeService.OnThemeChanged += HandleThemeChanged;
    }

    /// <summary>
    /// Initializes theme state after first render.
    /// </summary>
    /// <param name="firstRender">Whether this is the first render pass.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var theme = await JS.InvokeAsync<string>("localStorage.getItem", ThemeKey);
            isNinetiesTheme = theme == "nineties";
            StateHasChanged();
        }
    }

    private async void HandleThemeChanged()
    {
        await UpdateThemeStatus();
    }

    /// <summary>
    /// Refreshes theme state and triggers rerender.
    /// </summary>
    /// <returns>A task that completes when the state update is finished.</returns>
    public async Task UpdateThemeStatus()
    {
        var theme = await JS.InvokeAsync<string>("localStorage.getItem", ThemeKey);
        isNinetiesTheme = theme == "nineties";
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Unsubscribes from theme notifications.
    /// </summary>
    public void Dispose()
    {
        ThemeService.OnThemeChanged -= HandleThemeChanged;
    }
}