//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Triggered WebJob that exports the joke data to Azure Blob Storage on a weekly schedule
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data;
using DadABase.Data.Repositories;
using DadABase.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DadABase.BackupWebJob;

/// <summary>
/// Entry point for the triggered backup WebJob.
/// </summary>
public static class Program
{
    /// <summary>
    /// Runs one backup export. Azure App Service invokes this executable on the schedule defined
    /// in settings.job ("0 0 3 * * 0" = every Sunday at 03:00 UTC). The process runs once and exits.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Zero when the export succeeded or was skipped; otherwise, one.</returns>
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddEnvironmentVariables();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            builder.Logging.AddApplicationInsights(
                telemetry => telemetry.ConnectionString = appInsightsConnectionString,
                _ => { });
        }

        var connectionString = builder.Configuration["AppSettings:DefaultConnection"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine("AppSettings:DefaultConnection is not configured - the backup export cannot run.");
            return 1;
        }

        builder.Services.AddDbContext<DadABaseDbContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddScoped<IJokeRepository, JokeSQLRepository>();
        builder.Services.AddScoped<IBackupExportService, BackupExportService>();
        builder.Services.AddScoped<IBackupMetadataRepository, BackupMetadataRepository>();
        builder.Services.AddSingleton<IBackupStorageService, BackupStorageService>();
        builder.Services.AddScoped<BackupExportJob>();

        using var host = builder.Build();
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BackupExportJob>>();

        try
        {
            var job = scope.ServiceProvider.GetRequiredService<BackupExportJob>();
            await job.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            // A non-zero exit code marks the WebJob run as failed in the App Service run history
            logger.LogError(ex, "Weekly backup export failed");
            return 1;
        }
    }
}
