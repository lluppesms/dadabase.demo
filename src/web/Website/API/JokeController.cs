//-----------------------------------------------------------------------
// <copyright file="JokeController.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke API Controller
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.API;

using DadABase.Data.Models;
using DadABase.Data.Repositories;
using DadABase.Web.Services;
using Microsoft.AspNetCore.Authorization;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

/// <summary>
/// Joke API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
[ApiKey]
//[Authorize] <- this forces the user to be logged in, Anonymous+ApiKey allows logged in access OR access with just an API key
public class JokeController : BaseAPIController
{
    #region Initialization
    /// <summary>
    /// Joke Repository
    /// </summary>
    public IJokeRepository JokeRepo { get; private set; }

    /// <summary>
    /// Rating user key resolver
    /// </summary>
    private readonly RatingUserKeyResolver _ratingKeyResolver;

    /// <summary>
    /// Joke API Controller (for unit tests)
    /// </summary>
    /// <param name="settings">Settings</param>
    /// <param name="contextAccessor">Context</param>
    /// <param name="jokeRepo">Repository</param>
    /// <param name="ratingKeyResolver">Rating user key resolver</param>
    public JokeController(AppSettings settings, IHttpContextAccessor contextAccessor, IJokeRepository jokeRepo, RatingUserKeyResolver ratingKeyResolver)
    {
        SetupAutoMapper();
        context = contextAccessor;
        AppSettingsValues = settings;
        AppSettingsValues.UserName = GetUserName();
        JokeRepo = jokeRepo;
        _ratingKeyResolver = ratingKeyResolver;
    }
    #endregion

    /// <summary>
    /// Get Random Joke
    /// </summary>
    /// <returns>Joke</returns>
    [HttpGet]
    public JokeBasic Get()
    {
        var userName = GetUserName();
        var joke = JokeRepo.GetRandomJoke(userName);
        var simplifiedJoke = iMapper.Map<JokeBasic>(joke);
        return simplifiedJoke;
    }

    /// <summary>
    /// Get List of Jokes
    /// </summary>
    /// <returns>Jokes</returns>
    [HttpGet]
    [Route("[action]")]
    public List<JokeBasic> List()
    {
        var userName = GetUserName();
        var jokes = JokeRepo.ListAll(userName);
        var simplifiedJokes = iMapper.Map<IEnumerable<Joke>, List<JokeBasic>>(jokes);
        return simplifiedJokes;
    }

    /// <summary>
    /// Get One Specific Joke
    /// </summary>
    /// <returns>Joke</returns>
    [HttpGet]
    [Route("{id}")]
    public JokeBasic GetOne(int id)
    {
        var userName = GetUserName();
        var joke = JokeRepo.GetOne(id, userName);
        var simplifiedJoke = iMapper.Map<JokeBasic>(joke);
        return simplifiedJoke;
    }

    /// <summary>
    /// Get Jokes by Category
    /// </summary>
    /// <param name="categoryTxt" example="Chickens">Category of Jokes</param>
    /// <returns>Jokes</returns>
    [HttpGet]
    [Route("category/{categoryTxt}")]
    public List<JokeBasic> Category(string categoryTxt)
    {
        var userName = GetUserName();
        var jokes = JokeRepo.SearchJokes(string.Empty, categoryTxt, userName);
        var simplifiedJokes = iMapper.Map<IEnumerable<Joke>, List<JokeBasic>>(jokes);
        return simplifiedJokes;
    }

    /// <summary>
    /// Search Jokes
    /// </summary>
    /// <param name="searchTxt" example="Bunny">A word that is in a joke</param>
    /// <returns>Jokes</returns>
    [HttpGet]
    [Route("search/{searchTxt}")]
    public List<JokeBasic> Search(string searchTxt)
    {
        var userName = GetUserName();
        var jokes = JokeRepo.SearchJokes(searchTxt, string.Empty, userName);
        var simplifiedJokes = iMapper.Map<IEnumerable<Joke>, List<JokeBasic>>(jokes);
        return simplifiedJokes;
    }

    /// <summary>
    /// Search Jokes within a Category
    /// </summary>
    /// <param name="categoryTxt" example="Chickens">Category of Jokes</param>
    /// <param name="searchTxt" example="Bunny">A word that is in a joke</param>
    /// <returns>Jokes</returns>
    [HttpGet]
    [Route("searchcategory/{categoryTxt}/{searchTxt}")]
    public List<JokeBasic> SearchCategory(string categoryTxt, string searchTxt)
    {
        var userName = GetUserName();
        var jokes = JokeRepo.SearchJokes(searchTxt, categoryTxt, userName);
        var simplifiedJokes = iMapper.Map<IEnumerable<Joke>, List<JokeBasic>>(jokes);
        return simplifiedJokes;
    }

    /// <summary>
    /// Submit or update a star rating (1–5) for a joke.
    /// Authenticated users are identified by their identity claim.
    /// Anonymous users are identified by a hash of their client IP.
    /// </summary>
    /// <param name="request">Rating request containing JokeId and UserRating.</param>
    /// <returns>Updated rating details.</returns>
    [HttpPost]
    [Route("rate")]
    [AllowAnonymous]
    public IActionResult Rate([FromBody] JokeRateRequest request)
    {
        if (request == null || request.JokeId <= 0 || request.UserRating < 1 || request.UserRating > 5)
        {
            return BadRequest(new { error = "JokeId must be positive and UserRating must be 1–5." });
        }

        var userName = GetUserName();
        var ratingKey = _ratingKeyResolver.Resolve(context?.HttpContext);

        var (success, userRating, averageRating, voteCount, wasInsert) =
            JokeRepo.SubmitOrUpdateRating(request.JokeId, request.UserRating, ratingKey, userName);

        if (!success)
        {
            return StatusCode(500, new { error = "Rating could not be saved." });
        }

        return Ok(new
        {
            jokeId = request.JokeId,
            userRating,
            averageRating,
            voteCount,
            wasInsert
        });
    }

    /// <summary>
    /// Returns the aggregate rating summary (average and vote count) for a joke.
    /// </summary>
    /// <param name="id">The joke identifier.</param>
    /// <returns>Average rating and vote count.</returns>
    [HttpGet]
    [Route("{id}/rating/summary")]
    [AllowAnonymous]
    public IActionResult RatingSummary(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "Invalid joke id." });
        }

        var (averageRating, voteCount) = JokeRepo.GetRatingSummaryForJoke(id);
        return Ok(new { jokeId = id, averageRating, voteCount });
    }

    /// <summary>
    /// Returns the current user's rating for a specific joke, or null if not yet rated.
    /// </summary>
    /// <param name="id">The joke identifier.</param>
    /// <returns>The current user's rating (1–5) or null.</returns>
    [HttpGet]
    [Route("{id}/rating/current")]
    [AllowAnonymous]
    public IActionResult UserRating(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "Invalid joke id." });
        }

        var ratingKey = _ratingKeyResolver.Resolve(context?.HttpContext);
        var rating = JokeRepo.GetUserRatingForJoke(id, ratingKey);
        return Ok(new { jokeId = id, userRating = rating });
    }
}