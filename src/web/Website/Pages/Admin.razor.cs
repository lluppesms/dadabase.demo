//-----------------------------------------------------------------------
// <copyright file="Admin.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Admin Page Code-Behind
// </summary>
//-----------------------------------------------------------------------
using DadABase.Web.Models.Application;

namespace DadABase.Web.Pages;

/// <summary>
/// Admin Page Code-Behind
/// </summary>
public partial class Admin : ComponentBase
{
    [Inject] AppSettings Settings { get; set; }
    [Inject] IConfiguration Configuration { get; set; }
    [Inject] HttpContextAccessor Context { get; set; }
    [Inject] IJSRuntime JsInterop { get; set; }
    //[Inject] BuildInfoService buildInfoService{ get; set; }

    private string userName = string.Empty;
    private string dataSource = string.Empty;
    private string apiKeyInfo = string.Empty;
    private string aiChatInfo = string.Empty;
    private string aiImageInfo = string.Empty;

    /// <summary>
    /// Initialization
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");
            var userIdentity = Context.HttpContext.User;
            userName = userIdentity != null ? userIdentity.Identity.Name : string.Empty;
            var isInAdminRole = userIdentity != null && userIdentity.IsInRole("Admin");
            if (isInAdminRole)
            {
                try
                {
                    //var buildInfo = await buildInfoService.GetBuildInfoAsync();
                    apiKeyInfo = string.IsNullOrEmpty(Settings.ApiKey) ? string.Empty : Settings.ApiKey[..1] + "...";
                    if (!string.IsNullOrEmpty(Settings.DefaultConnection))
                    {
                        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(Settings.DefaultConnection);
                        dataSource = $"SQL Server: {builder.DataSource}, Database: {builder.InitialCatalog}";
                    }
                    else
                    {
                        dataSource = "JSON File";
                    }

                    // Get AI Chat configuration
                    var aiChatEndpoint = Configuration["AppSettings:AzureOpenAI:Chat:Endpoint"];
                    var aiChatModel = Configuration["AppSettings:AzureOpenAI:Chat:DeploymentName"];
                    if (!string.IsNullOrEmpty(aiChatEndpoint) && !string.IsNullOrEmpty(aiChatModel))
                    {
                        var endpointUri = new Uri(aiChatEndpoint);
                        aiChatInfo = $"{endpointUri.Host} / {aiChatModel}";
                    }
                    else
                    {
                        aiChatInfo = "Not configured";
                    }

                    // Get AI Image configuration
                    var aiImageEndpoint = Configuration["AppSettings:AzureOpenAI:Image:Endpoint"];
                    var aiImageModel = Configuration["AppSettings:AzureOpenAI:Image:DeploymentName"];
                    if (!string.IsNullOrEmpty(aiImageEndpoint) && !string.IsNullOrEmpty(aiImageModel))
                    {
                        var endpointUri = new Uri(aiImageEndpoint);
                        aiImageInfo = $"{endpointUri.Host} / {aiImageModel}";
                    }
                    else
                    {
                        aiImageInfo = "Not configured";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading admin page! {ex.Message}");
                }
            }
            StateHasChanged();
        }
    }
    private string FormatBuildDate(string buildDate)
    {
        if (DateTime.TryParse(buildDate, out var date))
        {
            return $"Compiled {date.ToString("yyyy-MM-dd HH:mm:ss")}";
        }
        return buildDate;
    }
}
