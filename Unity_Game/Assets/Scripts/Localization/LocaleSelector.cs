using System;
using System.Collections;
using Save;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Localization
{
    public class LocaleSelector : MonoBehaviour
    {
        private static bool _active;
        public TMP_Dropdown localeDropdown;

        public void Start()
        {
            var storedLocaleId = SaveSystem.LoadLocale();
            if (storedLocaleId == -1) return;
            localeDropdown.value = storedLocaleId;
            ChangeLocale(storedLocaleId);
        }

        public void ChangeLocale(int localeId)
        {
            if (_active) return;
            StartCoroutine(SetLocale(localeId));
        }

        private static IEnumerator SetLocale(int localeId)
        {
            _active = true;
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeId];
            
            // Save to settings
            SaveSystem.SaveLocale(localeId);
            
            _active = false;
        }
    }
}