//-----------------------------------------------------------------------
// <copyright file="ConfigController.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Config API Controller
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.API;

using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

/// <summary>
/// Joke API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiKey]
//[Authorize] <- this forces the user to be logged in, Anonymous+ApiKey allows logged in access OR access with just an API key
public class ConfigController : BaseAPIController
{
    #region Initialization
    /// <summary>
    /// Config API Controller
    /// </summary>
    /// <param name="settings">Settings</param>
    /// <param name="contextAccessor">Context</param>
    public ConfigController(AppSettings settings, IHttpContextAccessor contextAccessor)
    {
        SetupAutoMapper();
        context = contextAccessor;
        AppSettingsValues = settings;
        AppSettingsValues.UserName = GetUserName();
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
        var userName = GetUserName();
        var isAdmin = IsAdmin();
        Console.WriteLine($"User {userName} called config api.  Admin: {isAdmin}");
        Console.WriteLine($"AppSettings.ApiKey={AppSettingsValues.ApiKey}");
        Console.WriteLine($"AppSettings.DefaultConnection={Utilities.SanitizeConnection(AppSettingsValues.DefaultConnection)}");
        Console.WriteLine($"AppSettings.EnvironmentName={AppSettingsValues.EnvironmentName}");
        Console.WriteLine($"AppSettings.ProjectEntities={Utilities.SanitizeConnection(AppSettingsValues.ProjectEntities)}");
        Console.WriteLine($"AppSettings.SuperUser={AppSettingsValues.SuperUserFirstName}={AppSettingsValues.SuperUserLastName}");
        Console.WriteLine($"AppSettings.VisualStudioTenantId={AppSettingsValues.VisualStudioTenantId}");
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
        return userName;
    }
}