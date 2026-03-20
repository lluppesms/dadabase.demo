//-----------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Utilities for Azure authentication and error handling
// </summary>
//-----------------------------------------------------------------------
using Azure.Core;
using Azure.Identity;

namespace JokeAnalyzer.Helpers;

/// <summary>
/// Utilities for Azure authentication and error handling
/// </summary>
public class Utilities
{
    /// <summary>
    /// Combines all the inner exception messages into one string
    /// </summary>
    public static string GetExceptionMessage(Exception ex)
    {
        var message = string.Empty;
        if (ex == null)
        {
            return message;
        }
        if (ex.Message != null)
        {
            message += ex.Message;
        }
        if (ex.InnerException == null)
        {
            return message;
        }
        if (ex.InnerException.Message != null)
        {
            message += " " + ex.InnerException.Message;
        }
        if (ex.InnerException.InnerException == null)
        {
            return message;
        }
        if (ex.InnerException.InnerException.Message != null)
        {
            message += " " + ex.InnerException.InnerException.Message;
        }
        if (ex.InnerException.InnerException.InnerException == null)
        {
            return message;
        }
        if (ex.InnerException.InnerException.InnerException.Message != null)
        {
            message += " " + ex.InnerException.InnerException.InnerException.Message;
        }
        return message;
    }

    /// <summary>
    /// Get Credentials for Azure authentication
    /// </summary>
    /// <param name="vsTenantId">Optional tenant ID for Visual Studio authentication</param>
    /// <returns>TokenCredential for Azure authentication</returns>
    public static TokenCredential GetCredentials(string vsTenantId = "")
    {
        var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

        try
        {
            // If service principal credentials are provided, use them explicitly
            if (!string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(tenantId))
            {
                Console.WriteLine("Using ClientSecretCredential for Azure authentication!");
                return new ClientSecretCredential(tenantId, clientId, clientSecret);
            }

            Console.WriteLine("Using DefaultAzureCredential for Azure authentication!");
            // Disable desktop-oriented credentials that require msalruntime/GUI deps so containers stay lean
            var options = new DefaultAzureCredentialOptions
            {
                ExcludeVisualStudioCredential = false,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeAzureCliCredential = false, // Keep CLI for local dev
                ExcludeManagedIdentityCredential = false, // Keep for Azure deployment
                ExcludeEnvironmentCredential = false, // Allow service principal via env vars
            };

            if (!string.IsNullOrEmpty(vsTenantId))
            {
                options.TenantId = vsTenantId; // Force tenant to avoid mismatch errors in local dev
            }

            return new DefaultAzureCredential(options);
        }
        catch (Exception ex)
        {
            var message = GetExceptionMessage(ex);
            Console.WriteLine("GetCredentials Failed: " + message);
            throw;
        }
    }
}
