namespace DesignStudio.Localization;

public interface ILocalizationService
{
    Language CurrentLanguage { get; }
    event EventHandler? LanguageChanged;
    void SetLanguage(Language language);
    string Get(string key);
}
