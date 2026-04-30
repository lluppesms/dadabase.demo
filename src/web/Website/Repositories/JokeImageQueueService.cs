//-----------------------------------------------------------------------
// <copyright file="JokeImageQueueService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Background service that generates AI scene descriptions and images for queued jokes
// </summary>
//-----------------------------------------------------------------------
using System.Collections.Concurrent;
using DadABase.Data.Repositories;

namespace DadABase.Web.Repositories;

/// <summary>
/// In-memory queue implementation for scheduling deferred joke image generation.
/// </summary>
public class JokeImageQueue : IJokeImageQueue
{
    private readonly ConcurrentQueue<int> _queue = new();

    /// <inheritdoc />
    public void Enqueue(int jokeId) => _queue.Enqueue(jokeId);

    /// <inheritdoc />
    public bool TryDequeue(out int jokeId) => _queue.TryDequeue(out jokeId);

    /// <inheritdoc />
    public int Count => _queue.Count;
}

/// <summary>
/// Hosted background service that picks up joke IDs from the <see cref="IJokeImageQueue"/>
/// and generates AI scene descriptions and images for each one.
/// </summary>
public class JokeImageQueueService : BackgroundService
{
    private readonly IJokeImageQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JokeImageQueueService> _logger;

    // Delay between polling intervals when the queue is empty (seconds)
    private const int IdlePollSeconds = 10;
    // Delay between processing items to avoid rate-limiting (seconds)
    private const int ItemProcessDelaySeconds = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="JokeImageQueueService"/> class.
    /// </summary>
    public JokeImageQueueService(
        IJokeImageQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<JokeImageQueueService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JokeImageQueueService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var jokeId))
            {
                await ProcessJokeAsync(jokeId, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(ItemProcessDelaySeconds), stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(IdlePollSeconds), stoppingToken);
            }
        }

        _logger.LogInformation("JokeImageQueueService stopped.");
    }

    private async Task ProcessJokeAsync(int jokeId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing background image generation for JokeId {JokeId}.", jokeId);

        try
        {
            // Use a new DI scope for each item so scoped services (e.g. DbContext) are fresh
            using var scope = _scopeFactory.CreateScope();
            var jokeRepository = scope.ServiceProvider.GetRequiredService<IJokeRepository>();
            var aiHelper = scope.ServiceProvider.GetRequiredService<IAIHelper>();

            var joke = jokeRepository.GetOne(jokeId);
            if (joke == null)
            {
                _logger.LogWarning("JokeId {JokeId} not found — skipping image generation.", jokeId);
                return;
            }

            // Skip if a scene description already exists
            if (!string.IsNullOrWhiteSpace(joke.ImageTxt))
            {
                _logger.LogInformation("JokeId {JokeId} already has a scene description — skipping.", jokeId);
            }
            else
            {
                var categoryInfo = string.IsNullOrWhiteSpace(joke.Categories)
                    ? joke.JokeTxt
                    : $"{joke.JokeTxt} ({joke.Categories})";

                var (description, descSuccess, descMsg) = await aiHelper.GetJokeSceneDescription(categoryInfo);
                if (!descSuccess || string.IsNullOrWhiteSpace(description))
                {
                    _logger.LogWarning("Scene description generation failed for JokeId {JokeId}: {Message}", jokeId, descMsg);
                    return;
                }

                jokeRepository.UpdateImageTxt(jokeId, description, "BackgroundService");
                joke.ImageTxt = description;
                _logger.LogInformation("Scene description saved for JokeId {JokeId}.", jokeId);
            }

            // Skip if an image already exists in blob storage
            var existingImagePath = aiHelper.GetJokeImagePath(jokeId);
            if (!string.IsNullOrEmpty(existingImagePath))
            {
                _logger.LogInformation("Image already exists for JokeId {JokeId} — skipping.", jokeId);
                return;
            }

            var (imageUrl, imgSuccess, imgMsg) = await aiHelper.GenerateAnImage(joke.ImageTxt, jokeId);
            if (imgSuccess)
            {
                _logger.LogInformation("Image generated and saved for JokeId {JokeId}: {Url}", jokeId, imageUrl);
            }
            else
            {
                _logger.LogWarning("Image generation failed for JokeId {JokeId}: {Message}", jokeId, imgMsg);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing background image generation for JokeId {JokeId}.", jokeId);
        }
    }
}
