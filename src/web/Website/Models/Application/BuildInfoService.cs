namespace DadABase.Web.Models.Application;

using Newtonsoft.Json;

/// <summary>
/// Build Info Service Interface
/// </summary>
public interface IBuildInfoService
{
    /// <summary>
    /// Get Build Info
    /// </summary>
    Task<BuildInfo> GetBuildInfoAsync();
}

/// <summary>
/// Build Info Service
/// </summary>
public class BuildInfoService : IBuildInfoService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<BuildInfoService> _logger;
    private BuildInfo _cachedBuildInfo;

    /// <summary>
    /// Constructor
    /// </summary>
    public BuildInfoService(IWebHostEnvironment webHostEnvironment, ILogger<BuildInfoService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    /// <summary>
    /// Get Build Info
    /// </summary>
    public async Task<BuildInfo> GetBuildInfoAsync()
    {
        if (_cachedBuildInfo is not null)
        {
            return _cachedBuildInfo;
        }

        try
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "buildinfo.json");

            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                _cachedBuildInfo = JsonConvert.DeserializeObject<BuildInfo>(json);
                return _cachedBuildInfo;
            }
            else
            {
                _logger.LogInformation("BuildInfo.json file not found at {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load build info from file");
        }

        return null;
    }
}
