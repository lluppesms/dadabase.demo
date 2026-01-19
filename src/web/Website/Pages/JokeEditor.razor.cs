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

    private List<Joke> allJokes;
    private IEnumerable<Joke> filteredJokes;
    private List<JokeCategory> allCategories;
    private Joke editingJoke;
    private IEnumerable<int> selectedCategoryIds = new HashSet<int>();

    private bool isLoading = true;
    private bool isSaving = false;
    private string searchText = string.Empty;
    private string categoryFilter = string.Empty;
    private string editMessage = string.Empty;
    private string editAlertClass = "alert-info";

    // MudDataGrid sorting
    private Func<Joke, object> _sortByJokeText = x => x.JokeTxt;

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
    /// Start editing a joke
    /// </summary>
    private void EditJoke(int jokeId)
    {
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
                Categories = joke.Categories
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

            if (selectedCategoryIds.Count == 0)
            {
                editMessage = "At least one category must be selected.";
                editAlertClass = "alert-danger";
                isSaving = false;
                StateHasChanged();
                return;
            }

            // Save joke
            var success = JokeRepository.UpdateJoke(editingJoke);
            if (!success)
            {
                editMessage = "Failed to update joke.";
                editAlertClass = "alert-danger";
                isSaving = false;
                StateHasChanged();
                return;
            }

            // Update categories
            success = JokeRepository.UpdateJokeCategories(editingJoke.JokeId, selectedCategoryIds);
            if (!success)
            {
                editMessage = "Joke updated, but failed to update categories.";
                editAlertClass = "alert-warning";
                isSaving = false;
                StateHasChanged();
                return;
            }

            editMessage = "Joke updated successfully!";
            editAlertClass = "alert-success";

            // Reload data and return to list
            await Task.Delay(1500); // Show success message briefly
            LoadData();
            editingJoke = null;
            FilterJokes();
        }
        catch (Exception ex)
        {
            editMessage = $"Error saving joke: {ex.Message}";
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
        selectedCategoryIds.Clear();
        StateHasChanged();
    }
}
