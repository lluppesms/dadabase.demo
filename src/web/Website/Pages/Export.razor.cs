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
    [Inject] IJokeRepository JokeRepository { get; set; }

    private string statusMessage = string.Empty;
    private string alertClass = "alert-info";
    private bool isExporting = false;

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
            isExporting = true;
            statusMessage = "Generating export file...";
            alertClass = "alert-info";
            StateHasChanged();

            // Generate the SQL export content directly from the repository
            var sqlContent = JokeRepository.ExportToSql();
            
            // Create a stream from the SQL content
            var byteArray = System.Text.Encoding.UTF8.GetBytes(sqlContent);
            
            // Generate filename with timestamp
            var fileName = $"JokeExport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
            
            // Use the existing downloadFileFromStream JavaScript function
            using (var stream = new MemoryStream(byteArray))
            using (var streamRef = new DotNetStreamReference(stream: stream))
            {
                await JsInterop.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
            }

            statusMessage = $"Export file '{fileName}' downloaded successfully!";
            alertClass = "alert-success";
        }
        catch (Exception ex)
        {
            statusMessage = $"Error generating export: {Helpers.Utilities.GetExceptionMessage(ex)}";
            alertClass = "alert-danger";
        }
        finally
        {
            isExporting = false;
            StateHasChanged();
        }
    }
}
