//-----------------------------------------------------------------------
// <copyright file="JokeEditor.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Joke Editor Page Code-Behind
// </summary>
//-----------------------------------------------------------------------
using MudBlazor;

namespace DadABase.Web.Pages;

/// <summary>
/// Joke Editor Page Code-Behind
/// </summary>
public partial class JokeEditor : ComponentBase
{
    [Inject] IJokeRepository JokeRepository { get; set; }
    [Inject] IJSRuntime JsInterop { get; set; }
    [Inject] HttpContextAccessor Context { get; set; }

    private List<Joke> allJokes;
    private IEnumerable<Joke> filteredJokes;
    private List<JokeCategory> allCategories;
    private Joke editingJoke;
    private IEnumerable<int> selectedCategoryIds = new HashSet<int>();
    private string currentUserName = string.Empty;
    private int userTimezoneOffsetMinutes = 0;

    private bool isLoading = true;
    private bool isSaving = false;
    private bool isAddingNew = false;
    private string searchText = string.Empty;
    private string categoryFilter = string.Empty;
    private string editMessage = string.Empty;
    private string editAlertClass = "alert-info";

    // MudDataGrid sorting
    private Func<Joke, object> _sortByJokeText = x => x.JokeTxt;

    /// <summary>
    /// Convert UTC DateTime to user's local time
    /// </summary>
    private string FormatLocalDateTime(DateTime utcDateTime)
    {
        // JavaScript getTimezoneOffset() returns positive for behind UTC, negative for ahead
        // So we subtract the offset to get local time
        var localDateTime = utcDateTime.AddMinutes(-userTimezoneOffsetMinutes);
        return localDateTime.ToString("yyyy-MM-dd hh:mm tt");
    }

    /// <summary>
    /// Initialization
    /// </summary>
    protected override void OnInitialized()
    {
        LoadData();
    }

    /// <summary>
    /// After render
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");
            var userIdentity = Context.HttpContext?.User;
            currentUserName = userIdentity?.Identity?.Name ?? "UNKNOWN";

            // Get user's timezone offset (returns minutes, negative for ahead of UTC)
            userTimezoneOffsetMinutes = await JsInterop.InvokeAsync<int>("eval", "new Date().getTimezoneOffset()");
            StateHasChanged();
        }
    }

    /// <summary>
    /// Load all jokes and categories
    /// </summary>
    private void LoadData()
    {
        isLoading = true;
        StateHasChanged();

        allJokes = JokeRepository.ListAll("Y").ToList();
        allCategories = JokeRepository.GetAllCategories().ToList();
        filteredJokes = allJokes;

        isLoading = false;
        StateHasChanged();
    }

    /// <summary>
    /// Filter and sort jokes based on current criteria
    /// </summary>
    private void FilterJokes()
    {
        var query = allJokes.AsEnumerable();

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(j => j.JokeTxt?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(categoryFilter))
        {
            query = query.Where(j => j.Categories?.Contains(categoryFilter, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        filteredJokes = query.ToList();
        StateHasChanged();
    }

    /// <summary>
    /// Get display text for selected categories in multi-select
    /// </summary>
    private string GetSelectedCategoriesText(List<string> selectedValues)
    {
        var selectedIds = selectedCategoryIds.ToList();
        if (selectedIds.Count == 0)
            return "Select categories...";

        var selectedNames = allCategories
            .Where(c => selectedIds.Contains(c.JokeCategoryId))
            .Select(c => c.JokeCategoryTxt);

        return string.Join(", ", selectedNames);
    }

    /// <summary>
    /// Get alert severity based on alert class
    /// </summary>
    private Severity GetAlertSeverity()
    {
        return editAlertClass switch
        {
                "alert-success" => Severity.Success,
                "alert-warning" => Severity.Warning,
                "alert-danger" => Severity.Error,
                _ => Severity.Info
            };
        }

        /// <summary>
        /// Start adding a new joke
        /// </summary>
        private void AddNewJoke()
        {
            isAddingNew = true;
            editingJoke = new Joke
            {
                JokeId = 0,
                JokeTxt = string.Empty,
                Attribution = string.Empty,
                ImageTxt = string.Empty,
                ActiveInd = "Y",
                SortOrderNbr = 50
            };
            selectedCategoryIds = new HashSet<int>();
            editMessage = string.Empty;
            StateHasChanged();
        }

        /// <summary>
        /// Start editing a joke
        /// </summary>
        private void EditJoke(int jokeId)
        {
            isAddingNew = false;
            var joke = JokeRepository.GetOne(jokeId);
            if (joke != null)
            {
            editingJoke = new Joke
            {
                JokeId = joke.JokeId,
                JokeTxt = joke.JokeTxt,
                Attribution = joke.Attribution,
                ImageTxt = joke.ImageTxt,
                ActiveInd = joke.ActiveInd,
                SortOrderNbr = joke.SortOrderNbr,
                Categories = joke.Categories,
                CreateUserName = joke.CreateUserName,
                CreateDateTime = joke.CreateDateTime,
                ChangeUserName = joke.ChangeUserName,
                ChangeDateTime = joke.ChangeDateTime
            };

            // Parse categories to get selected category IDs
            var categoryIds = new HashSet<int>();
            if (!string.IsNullOrEmpty(joke.Categories))
            {
                var categoryNames = joke.Categories.Split(',').Select(c => c.Trim());
                foreach (var categoryName in categoryNames)
                {
                    var category = allCategories.FirstOrDefault(c =>
                        c.JokeCategoryTxt.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                    if (category != null)
                    {
                        categoryIds.Add(category.JokeCategoryId);
                    }
                }
            }
            selectedCategoryIds = categoryIds;

            editMessage = string.Empty;
        }
        StateHasChanged();
    }

    /// <summary>
    /// Save the edited joke
    /// </summary>
    private async Task SaveJoke()
    {
        if (editingJoke == null)
        {
            return;
        }

        isSaving = true;
        editMessage = "Saving...";
        editAlertClass = "alert-info";
        StateHasChanged();

        try
        {
            // Validate
            if (string.IsNullOrWhiteSpace(editingJoke.JokeTxt))
            {
                editMessage = "Joke text is required.";
                editAlertClass = "alert-danger";
                isSaving = false;
                StateHasChanged();
                return;
            }

            if (selectedCategoryIds.Count() == 0)
            {
                editMessage = "At least one category must be selected.";
                editAlertClass = "alert-danger";
                isSaving = false;
                StateHasChanged();
                return;
            }

            // Save joke (add or update)
            editingJoke.ActiveInd = "Y";
            int jokeId;
            bool success;

            if (isAddingNew)
            {
                // Add new joke
                jokeId = JokeRepository.AddJoke(editingJoke, currentUserName);
                if (jokeId < 0)
                {
                    editMessage = "Failed to add joke.";
                    editAlertClass = "alert-danger";
                    isSaving = false;
                    StateHasChanged();
                    return;
                }
                success = true;
            }
            else
            {
                // Update existing joke
                jokeId = editingJoke.JokeId;
                success = JokeRepository.UpdateJoke(editingJoke, currentUserName);
                if (!success)
                {
                    editMessage = "Failed to update joke.";
                    editAlertClass = "alert-danger";
                    isSaving = false;
                    StateHasChanged();
                    return;
                }
            }

            // Update categories
            success = JokeRepository.UpdateJokeCategories(jokeId, selectedCategoryIds.ToList(), currentUserName);
            if (!success)
            {
                editMessage = isAddingNew ? "Joke added, but failed to update categories." : "Joke updated, but failed to update categories.";
                editAlertClass = "alert-warning";
                isSaving = false;
                StateHasChanged();
                return;
            }

            editMessage = isAddingNew ? "Joke added successfully!" : "Joke updated successfully!";
            editAlertClass = "alert-success";

            // Reload data and return to list
            await Task.Delay(1500); // Show success message briefly
            LoadData();
            editingJoke = null;
            isAddingNew = false;
            FilterJokes();
        }
        catch (Exception ex)
        {
            editMessage = $"Error saving joke: {Helpers.Utilities.GetExceptionMessage(ex)}";
            editAlertClass = "alert-danger";
        }
        finally
        {
            isSaving = false;
            StateHasChanged();
        }
    }

    /// <summary>
        /// Cancel editing
        /// </summary>
        private void CancelEdit()
        {
            editingJoke = null;
            editMessage = string.Empty;
            isAddingNew = false;
            selectedCategoryIds = new HashSet<int>();
            StateHasChanged();
        }
    }
