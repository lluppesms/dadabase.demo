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
    [Inject] IDialogService DialogService { get; set; }
    [Inject] IAIHelper GenAIAgent { get; set; }

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

    private int currentWizardStep = 0;
    private bool isSuggestingCategories = false;
    private bool isGeneratingScenario = false;
    private bool isGeneratingImage = false;
    private string jokeImageUrl = string.Empty;

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
    /// Get the MudBlazor color for a wizard step chip
    /// </summary>
    private Color GetStepColor(int step)
    {
        if (step < currentWizardStep) return Color.Success;
        if (step == currentWizardStep) return Color.Primary;
        return Color.Default;
    }

    /// <summary>
    /// Start adding a new joke - resets wizard to step 0
    /// </summary>
    private void AddNewJoke()
    {
        isAddingNew = true;
        currentWizardStep = 0;
        jokeImageUrl = string.Empty;
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
        currentWizardStep = 0;
        jokeImageUrl = GenAIAgent.GetJokeImagePath(jokeId);
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
    /// Advance to the next wizard step - validates and reveals the image section
    /// </summary>
    private async Task NextWizardStep()
    {
        editMessage = string.Empty;

        if (currentWizardStep == 0)
        {
            if (string.IsNullOrWhiteSpace(editingJoke.JokeTxt))
            {
                editMessage = "Joke text is required.";
                editAlertClass = "alert-danger";
                StateHasChanged();
                return;
            }
            if (!selectedCategoryIds.Any())
            {
                editMessage = "Please select at least one category, or click Auto-Assign to have AI suggest categories.";
                editAlertClass = "alert-warning";
                StateHasChanged();
                return;
            }
            currentWizardStep = 1;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Go back to the previous wizard step
    /// </summary>
    private void PreviousWizardStep()
    {
        if (currentWizardStep > 0)
        {
            currentWizardStep--;
            editMessage = string.Empty;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Use AI to analyze the joke and get both category suggestions and scene description in one call
    /// </summary>
    private async Task AnalyzeJokeAsync()
    {
        isSuggestingCategories = true;
        isGeneratingScenario = true;
        StateHasChanged();

        try
        {
            var (suggestedNames, description, success, message) = await GenAIAgent.AnalyzeJoke(
                editingJoke.JokeTxt,
                allCategories.Select(c => c.JokeCategoryTxt));

            if (success)
            {
                // Set suggested categories
                if (suggestedNames.Count > 0)
                {
                    var matchedIds = allCategories
                        .Where(c => suggestedNames.Any(s => s.Equals(c.JokeCategoryTxt, StringComparison.OrdinalIgnoreCase)))
                        .Select(c => c.JokeCategoryId);
                    selectedCategoryIds = new HashSet<int>(matchedIds);
                }

                // Set scene description
                if (!string.IsNullOrWhiteSpace(description))
                {
                    editingJoke.ImageTxt = description;
                }
            }
            else if (!string.IsNullOrEmpty(message))
            {
                editMessage = message;
                editAlertClass = "alert-warning";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Joke analysis failed: {Helpers.Utilities.GetExceptionMessage(ex)}");
            editMessage = "Unable to analyze joke. Please select categories and enter a description manually, or try again.";
            editAlertClass = "alert-warning";
        }
        finally
        {
            isSuggestingCategories = false;
            isGeneratingScenario = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Ask AI to suggest categories for the joke
    /// </summary>
    private async Task SuggestCategoriesAsync()
    {
        isSuggestingCategories = true;
        StateHasChanged();

        try
        {
            var (suggestedNames, success, message) = await GenAIAgent.SuggestCategories(
                editingJoke.JokeTxt,
                allCategories.Select(c => c.JokeCategoryTxt));

            if (success && suggestedNames.Count > 0)
            {
                var matchedIds = allCategories
                    .Where(c => suggestedNames.Any(s => s.Equals(c.JokeCategoryTxt, StringComparison.OrdinalIgnoreCase)))
                    .Select(c => c.JokeCategoryId);
                selectedCategoryIds = new HashSet<int>(matchedIds);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Category suggestion failed: {Helpers.Utilities.GetExceptionMessage(ex)}");
            editMessage = "Unable to suggest categories. Please try again or select categories manually.";
            editAlertClass = "alert-warning";
        }
        finally
        {
            isSuggestingCategories = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Build the joke-with-categories string used as input to the scene description AI
    /// </summary>
    private string GetJokeWithCategories() =>
        $"{editingJoke.JokeTxt} ({GetSelectedCategoriesText(null)})";

    /// <summary>
    /// Ask AI to generate a scene description for the joke (wizard mode)
    /// </summary>
    private async Task GenerateScenarioAsync()
    {
        isGeneratingScenario = true;
        StateHasChanged();

        try
        {
            var (description, success, message) = await GenAIAgent.GetJokeSceneDescription(GetJokeWithCategories());
            if (success && !string.IsNullOrWhiteSpace(description))
            {
                editingJoke.ImageTxt = description;
            }
            else if (!success)
            {
                editMessage = message;
                editAlertClass = "alert-warning";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Scenario generation failed: {Helpers.Utilities.GetExceptionMessage(ex)}");
            editMessage = "Unable to generate a scenario description. You can enter one manually or try again.";
            editAlertClass = "alert-warning";
        }
        finally
        {
            isGeneratingScenario = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Skip image generation and proceed to save
    /// </summary>
    private void SkipImageGeneration()
    {
        // Set a placeholder to indicate image was intentionally skipped
        jokeImageUrl = "skipped";
        editingJoke.ImageTxt = string.Empty;
        StateHasChanged();
    }

    /// <summary>
    /// Clear the wizard image and allow user to regenerate or skip
    /// </summary>
    private void ClearWizardImage()
    {
        jokeImageUrl = string.Empty;
        StateHasChanged();
    }

    /// <summary>
    /// Regenerate the wizard image (clears current and regenerates from existing description)
    /// </summary>
    private async Task RegenerateWizardImageAsync()
    {
        jokeImageUrl = string.Empty;
        StateHasChanged();
        await GenerateImageAsync();
    }

    /// <summary>
    /// Create an image for the joke: generates scene description then image in one action
    /// </summary>
    private async Task CreateJokeImageAsync()
    {
        editMessage = string.Empty;

        // Step 1: Generate scene description if not already set
        if (string.IsNullOrWhiteSpace(editingJoke.ImageTxt))
        {
            await GenerateScenarioAsync();
        }

        // Step 2: Generate image from the description
        if (!string.IsNullOrWhiteSpace(editingJoke.ImageTxt))
        {
            await GenerateImageAsync();
        }
    }

    /// <summary>
    /// Ask AI to generate an image for the joke (wizard mode, jokeId=0 before saving)
    /// </summary>
    private async Task GenerateImageAsync()
    {
        if (string.IsNullOrWhiteSpace(editingJoke.ImageTxt))
        {
            jokeImageUrl = string.Empty;
            return;
        }

        isGeneratingImage = true;
        StateHasChanged();

        try
        {
            // Use jokeId=0 for new jokes; image will be regenerated with real ID after save
            var (imageUrl, success, message) = await GenAIAgent.GenerateAnImage(editingJoke.ImageTxt, 0);
            if (success)
            {
                jokeImageUrl = imageUrl;
            }
            else
            {
                editMessage = message;
                editAlertClass = "alert-warning";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Image generation failed: {Helpers.Utilities.GetExceptionMessage(ex)}");
            editMessage = "Unable to generate an image. You can continue and save the joke without an image.";
            editAlertClass = "alert-warning";
        }
        finally
        {
            isGeneratingImage = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Re-generate scene description in edit mode and persist it to the database
    /// </summary>
    private async Task RegenerateScenarioAsync()
    {
        isGeneratingScenario = true;
        editMessage = string.Empty;
        StateHasChanged();

        try
        {
            var (description, success, message) = await GenAIAgent.GetJokeSceneDescription(GetJokeWithCategories());
            if (success && !string.IsNullOrWhiteSpace(description))
            {
                editingJoke.ImageTxt = description;
                JokeRepository.UpdateImageTxt(editingJoke.JokeId, description, currentUserName);
            }
            else if (!success)
            {
                editMessage = message;
                editAlertClass = "alert-warning";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Scenario regeneration failed: {Helpers.Utilities.GetExceptionMessage(ex)}");
            editMessage = "Unable to regenerate scenario. Please try again or edit the description manually.";
            editAlertClass = "alert-warning";
        }
        finally
        {
            isGeneratingScenario = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Re-generate image in edit mode using the real joke ID
    /// </summary>
    private async Task RegenerateImageAsync()
    {
        if (string.IsNullOrWhiteSpace(editingJoke.ImageTxt))
        {
            jokeImageUrl = string.Empty;
            return;
        }

        isGeneratingImage = true;
        editMessage = string.Empty;
        StateHasChanged();

        try
        {
            var (imageUrl, success, message) = await GenAIAgent.GenerateAnImage(editingJoke.ImageTxt, editingJoke.JokeId);
            if (success)
            {
                jokeImageUrl = imageUrl;
            }
            else
            {
                editMessage = message;
                editAlertClass = "alert-warning";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Image regeneration failed: {Helpers.Utilities.GetExceptionMessage(ex)}");
            editMessage = "Unable to regenerate image. Please try again.";
            editAlertClass = "alert-warning";
        }
        finally
        {
            isGeneratingImage = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Save the edited or new joke
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

            if (!selectedCategoryIds.Any())
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
            var imageGenerationMessage = string.Empty;

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

                // Update categories immediately after creating the joke
                success = JokeRepository.UpdateJokeCategories(jokeId, selectedCategoryIds.ToList(), currentUserName);
                if (!success)
                {
                    editMessage = "Joke added, but failed to update categories.";
                    editAlertClass = "alert-warning";
                    isSaving = false;
                    StateHasChanged();
                    return;
                }

                // Persist the ImageTxt with the real ID, and save image to blob storage
                if (!string.IsNullOrWhiteSpace(editingJoke.ImageTxt))
                {
                    JokeRepository.UpdateImageTxt(jokeId, editingJoke.ImageTxt, currentUserName);
                }

                // Save the already-generated image to blob storage with the real jokeId
                // (avoids regenerating the image that was already created in the wizard)
                // Skip if user chose to skip image generation
                if (!string.IsNullOrWhiteSpace(jokeImageUrl) && jokeImageUrl != "skipped")
                {
                    var (blobUrl, imageSuccess, imageMsg) = await GenAIAgent.SaveBase64ImageToBlob(jokeImageUrl, jokeId);
                    if (imageSuccess)
                    {
                        jokeImageUrl = blobUrl;
                    }
                    else if (!string.IsNullOrEmpty(imageMsg))
                    {
                        // Non-fatal: joke is saved, capture message to display after
                        imageGenerationMessage = imageMsg;
                    }
                }
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

                // Update categories
                success = JokeRepository.UpdateJokeCategories(jokeId, selectedCategoryIds.ToList(), currentUserName);
                if (!success)
                {
                    editMessage = "Joke updated, but failed to update categories.";
                    editAlertClass = "alert-warning";
                    isSaving = false;
                    StateHasChanged();
                    return;
                }
            }

            // Display success message, including image generation warning if applicable
            if (!string.IsNullOrEmpty(imageGenerationMessage))
            {
                editMessage = $"Joke saved successfully, but image generation failed: {imageGenerationMessage}";
                editAlertClass = "alert-warning";
            }
            else
            {
                editMessage = isAddingNew ? "Joke added successfully!" : "Joke updated successfully!";
                editAlertClass = "alert-success";
            }

            // Brief delay to show success message
            StateHasChanged();
            await Task.Delay(1500);

            // Reload data from database to get updated categories and relationships
            LoadData();

            // Clear edit state and return to list
            editingJoke = null;
            isAddingNew = false;
            currentWizardStep = 0;
            jokeImageUrl = string.Empty;
            selectedCategoryIds = new HashSet<int>();

            // Apply current filters to the refreshed data
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
        currentWizardStep = 0;
        jokeImageUrl = string.Empty;
        selectedCategoryIds = new HashSet<int>();
        StateHasChanged();
    }

    /// <summary>
    /// Confirm delete joke with a dialog
    /// </summary>
    private async Task ConfirmDeleteJoke()
    {
        if (editingJoke == null)
        {
            return;
        }

        bool? result = await DialogService.ShowMessageBox(
            "Delete Joke",
            $"Are you sure you want to delete this joke? This action cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (result == true)
        {
            await DeleteJoke();
        }
    }

    /// <summary>
    /// Delete the joke
    /// </summary>
    private async Task DeleteJoke()
    {
        if (editingJoke == null)
        {
            return;
        }

        isSaving = true;
        editMessage = "Deleting...";
        editAlertClass = "alert-info";
        StateHasChanged();

        try
        {
            var jokeId = editingJoke.JokeId;
            var success = JokeRepository.DeleteJoke(jokeId, currentUserName);

            if (!success)
            {
                editMessage = "Failed to delete joke.";
                editAlertClass = "alert-danger";
                isSaving = false;
                StateHasChanged();
                return;
            }

            editMessage = "Joke deleted successfully!";
            editAlertClass = "alert-success";

            // Reload data and return to list
            await Task.Delay(1000); // Show success message briefly
            LoadData();
            editingJoke = null;
            isAddingNew = false;
            FilterJokes();
        }
        catch (Exception ex)
        {
            editMessage = $"Error deleting joke: {Helpers.Utilities.GetExceptionMessage(ex)}";
            editAlertClass = "alert-danger";
        }
        finally
        {
            isSaving = false;
            StateHasChanged();
        }
    }
}

