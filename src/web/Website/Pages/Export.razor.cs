//-----------------------------------------------------------------------
// <copyright file="Export.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Export Page Code-Behind
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Pages;

/// <summary>
/// Export Page Code-Behind
/// </summary>
public partial class Export : ComponentBase
{
    [Inject] AppSettings Settings { get; set; }
    [Inject] IJSRuntime JsInterop { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }

    private string statusMessage = string.Empty;
    private string alertClass = "alert-info";

    /// <summary>
    /// Initialization
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");
            StateHasChanged();
        }
    }

    /// <summary>
    /// Download export file
    /// </summary>
    private async Task DownloadExport()
    {
        try
        {
            statusMessage = "Generating export file...";
            alertClass = "alert-info";
            StateHasChanged();

            // Build the API URL based on the current base URL
            var baseUri = NavigationManager.BaseUri.TrimEnd('/');
            var apiUrl = $"{baseUri}/api/export/sql";
            await JsInterop.InvokeVoidAsync("downloadFile", apiUrl);

            statusMessage = "Export file download initiated successfully!";
            alertClass = "alert-success";
        }
        catch (Exception ex)
        {
            statusMessage = $"Error initiating download: {ex.Message}";
            alertClass = "alert-danger";
        }
        
        StateHasChanged();
    }
}
