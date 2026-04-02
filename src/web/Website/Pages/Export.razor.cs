//-----------------------------------------------------------------------
// <copyright file="Export.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Export / Import Page Code-Behind
// </summary>
//-----------------------------------------------------------------------
using Microsoft.AspNetCore.Components.Forms;

namespace DadABase.Web.Pages;

/// <summary>
/// Export / Import Page Code-Behind
/// </summary>
public partial class Export : ComponentBase
{
    [Inject] AppSettings Settings { get; set; }
    [Inject] IJSRuntime JsInterop { get; set; }
    [Inject] IJokeRepository JokeRepository { get; set; }

    // Shared busy state
    private bool isBusy = false;
    private string activeAction = string.Empty;

    // Maximum file size allowed for import uploads (10 MB)
    private const long MaxImportFileSizeBytes = 10 * 1024 * 1024;

    // Export status
    private string exportStatusMessage = string.Empty;
    private string exportAlertClass = "alert-info";

    // Import state
    private string importStatusMessage = string.Empty;
    private string importAlertClass = "alert-info";
    private string importFileName = string.Empty;
    private string importFileContent = null;
    private bool importReplaceAll = false;

    // Data source info
    private string dataSource = string.Empty;

    /// <summary>
    /// Initialization
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");

            if (!string.IsNullOrEmpty(Settings.DefaultConnection))
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(Settings.DefaultConnection);
                dataSource = $"SQL Server: {builder.DataSource}, Database: {builder.InitialCatalog}";
            }
            else
            {
                dataSource = "JSON File";
            }

            StateHasChanged();
        }
    }

    /// <summary>
    /// Downloads the SQL script export.
    /// </summary>
    private async Task DownloadSqlExport()
    {
        try
        {
            isBusy = true;
            activeAction = "sql";
            exportStatusMessage = "Generating SQL export file...";
            exportAlertClass = "alert-info";
            StateHasChanged();

            var sqlContent = JokeRepository.ExportToSql();
            var byteArray = Encoding.UTF8.GetBytes(sqlContent);
            var fileName = $"JokeExport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";

            using var stream = new MemoryStream(byteArray);
            using var streamRef = new DotNetStreamReference(stream: stream);
            await JsInterop.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

            exportStatusMessage = $"Export file '{fileName}' downloaded successfully!";
            exportAlertClass = "alert-success";
        }
        catch (Exception ex)
        {
            exportStatusMessage = $"Error generating SQL export: {Helpers.Utilities.GetExceptionMessage(ex)}";
            exportAlertClass = "alert-danger";
        }
        finally
        {
            isBusy = false;
            activeAction = string.Empty;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Downloads the tab-delimited export.
    /// </summary>
    private async Task DownloadTabExport()
    {
        try
        {
            isBusy = true;
            activeAction = "tab";
            exportStatusMessage = "Generating tab-delimited export file...";
            exportAlertClass = "alert-info";
            StateHasChanged();

            var tsvContent = JokeRepository.ExportToTabDelimited();
            var byteArray = Encoding.UTF8.GetBytes(tsvContent);
            var fileName = $"JokeExport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.tsv";

            using var stream = new MemoryStream(byteArray);
            using var streamRef = new DotNetStreamReference(stream: stream);
            await JsInterop.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

            exportStatusMessage = $"Export file '{fileName}' downloaded successfully!";
            exportAlertClass = "alert-success";
        }
        catch (Exception ex)
        {
            exportStatusMessage = $"Error generating tab-delimited export: {Helpers.Utilities.GetExceptionMessage(ex)}";
            exportAlertClass = "alert-danger";
        }
        finally
        {
            isBusy = false;
            activeAction = string.Empty;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Downloads the JSON export.
    /// </summary>
    private async Task DownloadJsonExport()
    {
        try
        {
            isBusy = true;
            activeAction = "json";
            exportStatusMessage = "Generating JSON export file...";
            exportAlertClass = "alert-info";
            StateHasChanged();

            var jsonContent = JokeRepository.ExportToJson();
            var byteArray = Encoding.UTF8.GetBytes(jsonContent);
            var fileName = $"JokeExport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

            using var stream = new MemoryStream(byteArray);
            using var streamRef = new DotNetStreamReference(stream: stream);
            await JsInterop.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

            exportStatusMessage = $"Export file '{fileName}' downloaded successfully!";
            exportAlertClass = "alert-success";
        }
        catch (Exception ex)
        {
            exportStatusMessage = $"Error generating JSON export: {Helpers.Utilities.GetExceptionMessage(ex)}";
            exportAlertClass = "alert-danger";
        }
        finally
        {
            isBusy = false;
            activeAction = string.Empty;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Reads the selected import file into memory.
    /// </summary>
    private async Task OnImportFileSelected(InputFileChangeEventArgs e)
    {
        importStatusMessage = string.Empty;
        importFileName = string.Empty;
        importFileContent = null;

        var file = e.File;
        if (file == null) return;

        try
        {
            // Limit to 10 MB to guard against unexpectedly large uploads
            using var reader = new StreamReader(file.OpenReadStream(maxAllowedSize: MaxImportFileSizeBytes));
            importFileContent = await reader.ReadToEndAsync();
            importFileName = file.Name;
        }
        catch (Exception ex)
        {
            importStatusMessage = $"Error reading file: {Helpers.Utilities.GetExceptionMessage(ex)}";
            importAlertClass = "alert-danger";
        }

        StateHasChanged();
    }

    /// <summary>
    /// Runs the import using the loaded file content.
    /// </summary>
    private async Task RunImport()
    {
        if (string.IsNullOrEmpty(importFileContent)) return;

        // If Delete and Replace is checked, show a confirmation dialog
        if (importReplaceAll)
        {
            var confirmed = await JsInterop.InvokeAsync<bool>(
                "confirm",
                "WARNING: This will permanently delete ALL jokes, categories, and ratings in the database before importing.\n\nAre you sure you want to continue?"
            );
            if (!confirmed)
            {
                importStatusMessage = "Import cancelled by user.";
                importAlertClass = "alert-warning";
                StateHasChanged();
                return;
            }
        }

        try
        {
            isBusy = true;
            activeAction = "import";
            importStatusMessage = "Importing jokes...";
            importAlertClass = "alert-info";
            StateHasChanged();

            // Yield briefly so Blazor can re-render the spinner before the synchronous import runs
            await Task.Yield();

            // Auto-detect JSON format and convert to TSV for the stored procedure
            var importData = importFileContent;
            if (importData.TrimStart().StartsWith("["))
            {
                importData = DadABase.Data.Repositories.JokeSQLRepository.ConvertJsonToTabDelimited(importData);
            }
            else
            {
                // Normalize 9-column TSV exports back to 7-column format for the stored procedure
                importData = DadABase.Data.Repositories.JokeSQLRepository.NormalizeTabDelimitedForImport(importData);
            }

            var (success, importedCount, message) = JokeRepository.ImportFromTabDelimitedViaSproc(importData, importReplaceAll);

            importStatusMessage = message;
            importAlertClass = success ? "alert-success" : "alert-warning";
        }
        catch (Exception ex)
        {
            importStatusMessage = $"Error importing jokes: {Helpers.Utilities.GetExceptionMessage(ex)}";
            importAlertClass = "alert-danger";
        }
        finally
        {
            isBusy = false;
            activeAction = string.Empty;
            StateHasChanged();
        }
    }
}
