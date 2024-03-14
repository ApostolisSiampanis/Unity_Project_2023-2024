using System.Collections;
using Farm.Scripts.QuestSystem;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Farm.Scripts.Quests
{
    public class BeforeQuests : MonoBehaviour
    {
        [SerializeField] private Quest quest;
        [SerializeField] private LocalizeStringEvent localizeStringEvent;
        [SerializeField] private string titleKey;
        [SerializeField] private string descriptionKey;

        private void Start()
        {
            // Wait for the localization to be initialized.
            StartCoroutine(InitLocalization());

            // Get the localized strings.
            const string table = "BeforeQuestsFarmTable";

            // Quest title.
            localizeStringEvent.StringReference.SetReference(table, titleKey);
            string title = localizeStringEvent.StringReference.GetLocalizedString();

            // Quest description.
            localizeStringEvent.StringReference.SetReference(table, descriptionKey);
            string description = localizeStringEvent.StringReference.GetLocalizedString();

            // Set the title and description.
            quest.title = title;
            quest.description = description;
        }

        private static IEnumerator InitLocalization()
        {
            yield return LocalizationSettings.InitializationOperation;
        }
    }
}