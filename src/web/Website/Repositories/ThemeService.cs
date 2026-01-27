namespace DadABase.Web.Repositories;

public class ThemeService
{
    public event Action OnThemeChanged;
    public void NotifyThemeChanged()
    {
        OnThemeChanged?.Invoke();
    }
}
