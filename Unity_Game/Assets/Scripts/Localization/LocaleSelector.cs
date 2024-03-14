using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Localization
{
    public class LocaleSelector : MonoBehaviour
    {
        private static bool _active;

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
            _active = false;
        }
    }
}