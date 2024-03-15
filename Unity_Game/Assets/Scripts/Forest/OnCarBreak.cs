using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Forest
{
    public class OnCarBreak : MonoBehaviour
    {
        [SerializeField] private LocalizeStringEvent localizeStringEvent;

        public string GetMonologue()
        {
            // Wait for the localization to be initialized.
            StartCoroutine(InitLocalization());

            // Get the localized strings.
            const string table = "BeforeQuestsForestTable";

            // Alex monologue.
            localizeStringEvent.StringReference.SetReference(table, "on_car_break_monologue");
            return localizeStringEvent.StringReference.GetLocalizedString();
        }

        private static IEnumerator InitLocalization()
        {
            yield return LocalizationSettings.InitializationOperation;
        }
    }
}