//-----------------------------------------------------------------------
// <copyright file="ConfigController.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Config API Controller
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.API;

using Microsoft.AspNetCore.Authorization;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

/// <summary>
/// Config API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
//[ApiKey]
//[Authorize] <- this forces the user to be logged in, Anonymous+ApiKey allows logged in access OR access with just an API key
public class ConfigController : BaseAPIController
{
    IConfiguration _config;

    #region Initialization
    /// <summary>
    /// Config API Controller
    /// </summary>
    /// <param name="settings">Settings</param>
    /// <param name="contextAccessor">Context</param>
    /// <param name="config">Configuration</param>
    public ConfigController(AppSettings settings, IHttpContextAccessor contextAccessor, IConfiguration config)
    {
        SetupAutoMapper();
        context = contextAccessor;
        AppSettingsValues = settings;
        AppSettingsValues.UserName = GetUserName();
        _config = config;
        // _logger = logger;
    }
    #endregion

    /// <summary>
    /// Echoes configuration settings into the log for an admin to verify...
    /// </summary>
    /// <returns>User Name</returns>
    [HttpGet]
    public string Get()
    {
        string userName = "Unknown";
        bool isAdmin = false;
        try
        {
            userName = GetUserName();
            isAdmin = IsAdmin();
            Console.WriteLine($"User {userName} called config api. IsAdmin: {isAdmin}");
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error in ConfigController.Get - UserInfo: {msg}");
        }

        try
        {
            Console.WriteLine($"AppSettings.ApiKey={AppSettingsValues.ApiKey}");
            Console.WriteLine($"AppSettings.DefaultConnection={Utilities.SanitizeConnection(AppSettingsValues.DefaultConnection)}");
            Console.WriteLine($"AppSettings.EnvironmentName={AppSettingsValues.EnvironmentName}");
            Console.WriteLine($"AppSettings.AdminList={AppSettingsValues.AdminUserList}");
            Console.WriteLine($"AppSettings.VisualStudioTenantId={AppSettingsValues.VisualStudioTenantId}");
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error in ConfigController.Get - AppSettings: {msg}");
        }

        try
        {
            var openaiEndpoint = _config["AppSettings:AzureOpenAI:Chat:Endpoint"];
            var openaiDeploymentName = _config["AppSettings:AzureOpenAI:Chat:DeploymentName"];
            var openaiApiKey = _config["AppSettings:AzureOpenAI:Chat:ApiKey"];
            var openaiApiKeyMask = !string.IsNullOrEmpty(openaiApiKey) ? $"{openaiApiKey[..3]}... (~{openaiApiKey.Length} bytes)" : "(0 bytes)";
            var openaiMaxTokens = int.TryParse(_config["AppSettings:AzureOpenAI:Chat:MaxTokens"], out var parsedMaxTokens) ? parsedMaxTokens : 300;
            var openaiTemperature = float.TryParse(_config["AppSettings:AzureOpenAI:Chat:Temperature"], out var parsedTemperature) ? parsedTemperature : 0.7f;
            var openaiTopP = float.TryParse(_config["AppSettings:AzureOpenAI:Chat:TopP"], out var topP) ? topP : 0.95f;
            Console.WriteLine($"AppSettings:AzureOpenAI:Chat:Endpoint={openaiEndpoint}");
            Console.WriteLine($"AppSettings:AzureOpenAI:Chat:DeploymentName={openaiDeploymentName}");
            Console.WriteLine($"AppSettings:AzureOpenAI:Chat:ApiKey={openaiApiKeyMask}");
            Console.WriteLine($"AppSettings:AzureOpenAI:Chat:MaxTokens={openaiMaxTokens}");
            Console.WriteLine($"AppSettings:AzureOpenAI:Chat:Temperature={openaiTemperature}");
            Console.WriteLine($"AppSettings:AzureOpenAI:Chat:TopP={openaiTopP}");

            var openaiImageEndpoint = _config["AppSettings:AzureOpenAI:Image:Endpoint"];
            var openaiImageDeploymentName = _config["AppSettings:AzureOpenAI:Image:DeploymentName"];
            var openaiImageApiKey = _config["AppSettings:AzureOpenAI:Image:ApiKey"];
            var openaiImageApiKeyMask = !string.IsNullOrEmpty(openaiImageApiKey) ? $"{openaiImageApiKey[..3]}... (~{openaiImageApiKey.Length} bytes)" : "(0 bytes)";
            Console.WriteLine($"AppSettings:AzureOpenAI:Image:Endpoint={openaiImageEndpoint}");
            Console.WriteLine($"AppSettings:AzureOpenAI:Image:DeploymentName={openaiImageDeploymentName}");
            Console.WriteLine($"AppSettings:AzureOpenAI:Image:ApiKey={openaiImageApiKeyMask}");
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error in ConfigController.Get - AzureOpenAI: {msg}");
        }

        try
        {
            var buildInfoFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "buildinfo.json");
            if (System.IO.File.Exists(buildInfoFile))
            {
                using var r = new StreamReader(buildInfoFile);
                var buildInfoData = r.ReadToEnd();
                var buildInfoObject = JsonConvert.DeserializeObject<BuildInfo>(buildInfoData);
                Console.WriteLine($"build.BranchName={buildInfoObject.BranchName}");
                Console.WriteLine($"build.BuildDate={buildInfoObject.BuildDate}");
                Console.WriteLine($"build.BuildId={buildInfoObject.BuildId}");
                Console.WriteLine($"build.BuildNumber={buildInfoObject.BuildNumber}");
                Console.WriteLine($"build.BuildCommitHashNumber={buildInfoObject.CommitHash}");
            }
            else
            {
                Console.WriteLine($"{buildInfoFile} not found...!");
            }
        }
        catch (Exception ex)
        {
            var msg = Utilities.GetExceptionMessage(ex);
            Console.WriteLine($"Error in ConfigController.Get - BuildInfo: {msg}");
        }

        return userName;
    }
}