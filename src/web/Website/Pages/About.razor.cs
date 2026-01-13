//-----------------------------------------------------------------------
// <copyright file="About.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2025, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// About Page Code-Behind
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Web.Pages;

/// <summary>
/// About Page Code-Behind
/// </summary>
public partial class About : ComponentBase
{
    [Inject] IJSRuntime JsInterop { get; set; }
    [Inject] IConfiguration Config { get; set; }

    public string DatabaseType { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var connectionString = Config.GetSection("AppSettings")["DefaultConnection"];
        DatabaseType = !string.IsNullOrEmpty(connectionString) ? "SQL Dad-A-Base" : "JSON Dad-A-Base";
    }
    /// <summary>
    /// Initialization
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");
        }
    }
}
