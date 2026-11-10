using System.Globalization;
using System.Resources;

namespace sharpcommander.Localization;

/// <summary>Implemented by forms that have texts the generic name-based lookup cannot set.</summary>
internal interface ILocalizable
{
    void ApplyLocalization();
}

/// <summary>
/// Owns the UI language: resolves it at start-up, switches it at runtime, remembers the user's
/// choice and pushes translated texts into forms.
/// </summary>
/// <remarks>
/// To add a language, add <c>Strings.&lt;culture&gt;.resx</c> next to <c>Strings.resx</c> and list
/// the culture in <see cref="SupportedLanguages"/>.
/// </remarks>
internal static class Localizer
{
    private const string FallbackLanguage = "en";

    private static readonly ResourceManager Resources =
        new("sharpcommander.Localization.Strings", typeof(Localizer).Assembly);


    // The language Windows asked for, captured before we override the UI culture.
    private static readonly CultureInfo SystemUICulture = CultureInfo.CurrentUICulture;

    /// <summary>Languages with a translation; the first one is the neutral (built-in) language.</summary>
    public static IReadOnlyList<CultureInfo> SupportedLanguages { get; } =
    [
        CultureInfo.GetCultureInfo("hu"),
        CultureInfo.GetCultureInfo("en"),
    ];

    /// <summary>The language picked by the user, or <c>null</c> to follow the system language.</summary>
    public static string? SelectedLanguage { get; private set; }

    /// <summary>Loads the saved language choice and applies it. Call once, before any form is created.</summary>
    public static void Initialize()
    {
        SelectedLanguage = Normalize(AppSettings.Current.Language);
        ApplyCulture();
    }

    /// <summary>Switches the UI language, re-translates every open form and saves the choice.</summary>
    /// <param name="language">A culture name from <see cref="SupportedLanguages"/>, or <c>null</c> for the system language.</param>
    public static void SetLanguage(string? language)
    {
        SelectedLanguage = Normalize(language);
        ApplyCulture();
        AppSettings.Current.Language = SelectedLanguage;
        AppSettings.Current.Save();

        foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
        {
            Localize(form);
        }
    }

    public static string Get(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public static string Format(string key, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), args);

    /// <summary>
    /// Translates <paramref name="form"/>: every named control and tool strip item that has a
    /// <c>FormClass.name</c> (or <c>Common.name</c>) resource gets its text from it, then the
    /// form's own <see cref="ILocalizable.ApplyLocalization"/> runs. Safe to call repeatedly.
    /// </summary>
    public static void Localize(Form form)
    {
        string scope = form.GetType().Name;

        if (Find(scope, "$this") is { } caption)
        {
            form.Text = caption;
        }
        LocalizeControls(scope, form.Controls);

        (form as ILocalizable)?.ApplyLocalization();
    }

    /// <summary>Translates tool strip items (and their sub-items) that are created in code.</summary>
    public static void LocalizeItems(Form owner, ToolStripItemCollection items) =>
        LocalizeItems(owner.GetType().Name, items);

    private static void LocalizeControls(string scope, Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            // MDI children live inside the parent's MdiClient; they are forms with their own scope.
            if (control is Form or MdiClient)
            {
                continue;
            }

            if (control is ToolStrip toolStrip)
            {
                LocalizeItems(scope, toolStrip.Items);
            }
            else if (Find(scope, control.Name) is { } text)
            {
                control.Text = text;
            }

            LocalizeControls(scope, control.Controls);
        }
    }

    private static void LocalizeItems(string scope, ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            if (Find(scope, item.Name) is { } text)
            {
                item.Text = text;
            }
            if (Find(scope, item.Name + ".ToolTip") is { } toolTip)
            {
                item.ToolTipText = toolTip;
            }
            if (item is ToolStripDropDownItem dropDown)
            {
                LocalizeItems(scope, dropDown.DropDownItems);
            }
        }
    }

    private static string? Find(string scope, string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        CultureInfo culture = CultureInfo.CurrentUICulture;
        return Resources.GetString($"{scope}.{name}", culture) ?? Resources.GetString($"Common.{name}", culture);
    }

    private static void ApplyCulture()
    {
        CultureInfo culture = FindSupported(SelectedLanguage)
            ?? FindSupported(SystemUICulture.TwoLetterISOLanguageName)
            ?? CultureInfo.GetCultureInfo(FallbackLanguage);

        // Only the UI language changes; number and date formats keep following the system settings.
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture; // background workers show dialogs too
    }

    private static CultureInfo? FindSupported(string? language) =>
        SupportedLanguages.FirstOrDefault(c => string.Equals(c.Name, language, StringComparison.OrdinalIgnoreCase));

    private static string? Normalize(string? language) => FindSupported(language)?.Name;
}
