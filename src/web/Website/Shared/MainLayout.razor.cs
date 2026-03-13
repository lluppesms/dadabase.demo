//-----------------------------------------------------------------------
// <copyright file="MainLayout.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Main layout code-behind
// </summary>
//-----------------------------------------------------------------------
using System.Reflection;

namespace DadABase.Web.Shared;

/// <summary>
/// Provides state and behavior for the application's main layout.
/// </summary>
public partial class MainLayout : LayoutComponentBase, IDisposable
{
    private int thisYear = DateTime.Today.Year;
    private string assemblyVersionNumber => Assembly.GetEntryAssembly().GetName().Version.ToString();
    private BuildInfo buildInfo;
    private NinetiesThemeDecorations ninetiesDecorations;
    private NinetiesThemeConstructionBanner ninetiesConstructionBanner;
    private bool isNinetiesTheme = false;
    private const string ThemeKey = "theme-mode";

    /// <summary>
    /// Subscribes to theme change events.
    /// </summary>
    protected override void OnInitialized()
    {
        ThemeService.OnThemeChanged += HandleThemeChanged;
    }

    /// <summary>
    /// Loads build metadata for footer display.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        buildInfo = await BuildInfoService.GetBuildInfoAsync();
    }

    /// <summary>
    /// Initializes theme-specific rendering after first render.
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
        var theme = await JS.InvokeAsync<string>("localStorage.getItem", ThemeKey);
        isNinetiesTheme = theme == "nineties";
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Formats build date text for tooltip display.
    /// </summary>
    /// <param name="buildDate">The raw build date string.</param>
    /// <returns>Formatted build date text.</returns>
    private string FormatBuildDate(string buildDate)
    {
        if (DateTime.TryParse(buildDate, out var date))
        {
            return $"Compiled {date:yyyy-MM-dd HH:mm:ss}";
        }

        return buildDate;
    }

    /// <summary>
    /// Unsubscribes from theme change events.
    /// </summary>
    public void Dispose()
    {
        ThemeService.OnThemeChanged -= HandleThemeChanged;
    }
}