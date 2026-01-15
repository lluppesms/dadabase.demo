//-----------------------------------------------------------------------
// <copyright file="ModelConfiguration.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Configuration classes for AI model providers
// </summary>
//-----------------------------------------------------------------------
namespace JokeAnalyzer.Models;

/// <summary>
/// Specifies which AI model provider to use.
/// </summary>
public enum ModelProviderType
{
    /// <summary>
    /// Use a local model (e.g., Phi-4 via LM Studio).
    /// </summary>
    Local,

    /// <summary>
    /// Use Azure OpenAI (Azure Foundry) cloud model.
    /// </summary>
    AzureOpenAI
}

/// <summary>
/// Configuration for local AI models.
/// </summary>
public class LocalModelConfig
{
    public string Endpoint { get; set; } = "http://localhost:1234/v1";
    public string ModelId { get; set; } = "phi-4";
    public string ModelName { get; set; } = "phi-4";
}

/// <summary>
/// Configuration for Azure OpenAI models.
/// </summary>
public class AzureOpenAIConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
}
