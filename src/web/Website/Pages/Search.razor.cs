//-----------------------------------------------------------------------
// <copyright file="Search.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Search Code-Behind
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Pages;

/// <summary>
/// Search Page
/// </summary>
public partial class Search : ComponentBase
{
    [Inject] IJokeRepository JokeRepository { get; set; }
    [Inject] IJSRuntime JsInterop { get; set; }

    private string SearchTerm = string.Empty;
    private string SelectedCategory = "ALL";
    private IReadOnlyList<string> SelectedCategoryList = null;
    private List<Joke> myJokes = new();
    private List<string> JokeCategories = new();
    private readonly string AllJokesConstant = "ALL";

	/// <summary>
	/// Initialization
	/// </summary>
	protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");
            JokeCategories = JokeRepository.GetJokeCategories().ToList();
            SelectedCategory = AllJokesConstant;
            await JsInterop.InvokeVoidAsync("focusOnInputField", "inputText");
            StateHasChanged();
        }
    }

    //// if we want multi-select, use this...
    //private async Task OnMultiSelectValuesChanged(IEnumerable<string> values)
    //{
    //    SelectedCategoryList = values.ToList();
    //    // if first entry is ALL, remove all other entries
    //    if (SelectedCategoryList.Count > 0 && SelectedCategoryList[0] == "ALL")
    //    {
    //        SelectedCategoryList = new List<string> { "ALL" };
    //    }
    //    _ = await Task.FromResult(true);
    //}
    private async Task OnSelectedValueChanged(string value)
    {
        SelectedCategoryList = [value];
        _ = await Task.FromResult(true);
    }

    private async void ExecuteSearch()
    {
        var selectedCategories = !string.IsNullOrEmpty(SelectedCategory) ? SelectedCategory : AllJokesConstant;

        await JsInterop.InvokeVoidAsync("focusOnInputField", "btnSearch");
        await JsInterop.InvokeVoidAsync("focusOnInputField", "inputText");
        myJokes = JokeRepository.SearchJokes(SearchTerm, selectedCategories).ToList();
        StateHasChanged();
    }

    private void CheckForEnterKey(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            ExecuteSearch();
        }
    }
}
