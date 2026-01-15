//-----------------------------------------------------------------------
// <copyright file="ExportController.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Export API Controller
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.API;

using DadABase.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

/// <summary>
/// Export API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
[ApiKey]
public class ExportController : BaseAPIController
{
    #region Initialization
    /// <summary>
    /// Joke Repository
    /// </summary>
    public IJokeRepository JokeRepo { get; private set; }

    /// <summary>
    /// Export API Controller
    /// </summary>
    /// <param name="settings">Settings</param>
    /// <param name="contextAccessor">Context</param>
    /// <param name="jokeRepo">Repository</param>
    public ExportController(AppSettings settings, IHttpContextAccessor contextAccessor, IJokeRepository jokeRepo)
    {
        context = contextAccessor;
        AppSettingsValues = settings;
        AppSettingsValues.UserName = GetUserName();
        JokeRepo = jokeRepo;
    }
    #endregion

    /// <summary>
    /// Export all jokes to SQL file
    /// </summary>
    /// <returns>SQL file download</returns>
    [HttpGet]
    [Route("sql")]
    public IActionResult ExportSql()
    {
        var userName = GetUserName();
        var sqlContent = JokeRepo.ExportToSql(userName);
        
        var fileName = $"JokeExport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
        var contentType = "text/plain";
        
        return File(System.Text.Encoding.UTF8.GetBytes(sqlContent), contentType, fileName);
    }
}
