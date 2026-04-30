namespace DadABase.Web.Repositories;

/// <summary>
/// Service that broadcasts theme change notifications to subscribed components.
/// </summary>
public class ThemeService
{
    /// <summary>Raised when the active theme changes.</summary>
    public event Action OnThemeChanged;

    /// <summary>
    /// Invokes <see cref="OnThemeChanged"/> to notify all subscribers that the theme has changed.
    /// </summary>
    public void NotifyThemeChanged()
    {
        OnThemeChanged?.Invoke();
    }
}
