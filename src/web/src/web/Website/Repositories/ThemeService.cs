namespace DadABase.Web.Repositories;

/// <summary>
/// Service for managing theme state and notifying subscribers of theme changes
/// </summary>
public class ThemeService
{
    public event Action OnThemeChanged;

    public void NotifyThemeChanged()
    {
        OnThemeChanged?.Invoke();
    }
}
