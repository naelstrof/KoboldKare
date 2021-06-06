using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;

public class LocaleDropdown : MonoBehaviour, IGameEventOptionListener {
    public TMP_Dropdown dropdown;
    public GraphicsOptions goptions;

    IEnumerator Start()
    {
        goptions.RegisterListener(this);
        // Wait for the localization system to initialize, loading Locales, preloading etc.
        yield return LocalizationSettings.InitializationOperation;

        // Generate list of available Locales
        var options = new List<TMP_Dropdown.OptionData>();
        int selected = 0;
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales[i];
            if (LocalizationSettings.SelectedLocale == locale)
                selected = i;
            options.Add(new TMP_Dropdown.OptionData(locale.name));
        }
        dropdown.options = options;

        dropdown.value = selected;
        dropdown.onValueChanged.AddListener(LocaleSelected);
    }
    private void OnDestroy() {
        goptions.UnregisterListener(this);
    }

    void LocaleSelected(int index)
    {
        //LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        goptions.ChangeOption(GraphicsOptions.OptionType.Language, index);
    }

    public IEnumerator WaitForLocalizationToBeReadyThenCheck() {
        yield return LocalizationSettings.InitializationOperation;
        dropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
    }
    public void OnEventRaised(GraphicsOptions.OptionType e, float value) {
        if (e == GraphicsOptions.OptionType.Language) {
            StartCoroutine(WaitForLocalizationToBeReadyThenCheck());
        }
    }
}