//-----------------------------------------------------------------------
// <copyright file="Admin.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Admin Page Code-Behind
// </summary>
//-----------------------------------------------------------------------
using DadABase.Web.Helpers;

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
    [Inject] IJokeRepository JokeRepository { get; set; }
    //[Inject] BuildInfoService buildInfoService{ get; set; }

    private string userName = string.Empty;
    private string dataSource = string.Empty;
    private string apiKeyInfo = string.Empty;
    private string aiChatInfo = string.Empty;
    private string aiImageInfo = string.Empty;
    private bool isInAdminRole = false;

    private bool dataTestRunning = false;
    private bool dataTestSucceeded = false;
    private string dataTestMessage = string.Empty;
    private string dataTestError = string.Empty;

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
            isInAdminRole = userIdentity != null && userIdentity.IsInRole("Admin");
            if (isInAdminRole)
            {
                try
                {
                    var appDataSource = Configuration["AppSettings:DataSource"];

                    //var buildInfo = await buildInfoService.GetBuildInfoAsync();
                    apiKeyInfo = string.IsNullOrEmpty(Settings.ApiKey) ? string.Empty : Settings.ApiKey[..1] + "...";
                    if (!string.IsNullOrEmpty(Settings.DefaultConnection))
                    {
                        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(Settings.DefaultConnection);
                        dataSource = $"{appDataSource}: SQL Server: {builder.DataSource}, Database: {builder.InitialCatalog}";
                    }
                    else
                    {
                        dataSource = "{appDataSource}: JSON File";
                    }

                    // Get AI Chat configuration
                    var aiChatEndpoint = Configuration["AppSettings:AzureOpenAI:Chat:Endpoint"];
                    var aiChatModel = Configuration["AppSettings:AzureOpenAI:Chat:DeploymentName"];
                    var aiServiceProvider = Configuration["AppSettings:AiServiceProvider"];
                    if (!string.IsNullOrEmpty(aiChatEndpoint) && !string.IsNullOrEmpty(aiChatModel))
                    {
                        var endpointUri = new Uri(aiChatEndpoint);
                        aiChatInfo = $"{aiServiceProvider} -> {aiChatModel} -> {endpointUri.Host}";
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
                        aiImageInfo = $"{aiServiceProvider} -> {aiImageModel} -> {endpointUri.Host}";
                    }
                    else
                    {
                        aiImageInfo = "Not configured";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading admin page! {Utilities.GetExceptionMessage(ex)}");
                }

                await RunDataSourceTestAsync();
            }
            else
            {
                if (userIdentity == null)
                {
                    Console.WriteLine($"User tried to access the admin page but is not authenticated and therefore failed!");
                }
                else
                {
                    if (!string.IsNullOrEmpty(userName))
                    {
                        Console.WriteLine($"User {userName} tried to access the admin page but failed!");
                        Console.WriteLine($"Admin list = {Data.Constants.AdminUserList} ");
                        Console.WriteLine($"IsAdmin = {Data.Constants.AdminUserList.Contains(userName, StringComparison.InvariantCultureIgnoreCase)}");
                    }
                    else
                    {
                        Console.WriteLine($"User tried to access the admin page and seems to be authenticated but the userName is blank!");
                    }
                }
            }
            StateHasChanged();
        }
    }
    /// <summary>
    /// Verifies the configured data source actually works by requesting a single random joke.
    /// </summary>
    private async Task RunDataSourceTestAsync()
    {
        if (!isInAdminRole) { return; }

        dataTestRunning = true;
        dataTestSucceeded = false;
        dataTestMessage = string.Empty;
        dataTestError = string.Empty;
        StateHasChanged();

        try
        {
            var timer = Stopwatch.StartNew();
            var joke = await Task.Run(() => JokeRepository.GetRandomJoke(string.IsNullOrEmpty(userName) ? "ADMIN" : userName));
            timer.Stop();

            if (joke == null || string.IsNullOrWhiteSpace(joke.JokeTxt))
            {
                dataTestError = "The data source responded but returned no data.";
            }
            else
            {
                dataTestSucceeded = true;
                dataTestMessage = $"Retrieved joke #{joke.JokeId} in {timer.ElapsedMilliseconds} ms";
            }
        }
        catch (Exception ex)
        {
            dataTestError = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Admin data source test failed! {dataTestError}");
        }
        finally
        {
            dataTestRunning = false;
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
