using System.Collections;
using System.Collections.Generic;
using Common.DialogueSystem;
using Common.QuestSystem;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Forest.Quests.Car_Repair_Quest
{
    public class Localization : MonoBehaviour
    {
        [SerializeField] private Quest quest;
        [SerializeField] private LocalizeStringEvent localizeStringEvent;

        private void Start()
        {
            // Wait for the localization to be initialized.
            StartCoroutine(InitLocalization());

            // Get the localized strings.
            const string table = "CarRepairQuestTable";

            // Quest title.
            localizeStringEvent.StringReference.SetReference(table, "title");
            string title = localizeStringEvent.StringReference.GetLocalizedString();

            // Quest description.
            localizeStringEvent.StringReference.SetReference(table, "description");
            string description = localizeStringEvent.StringReference.GetLocalizedString();

            // Set the localized strings to the quest.
            // Set the title and description.
            quest.title = title;
            quest.description = description;

            // Update the quest UI.
            QuestManager.instance.UpdateQuestUI();
        }

        private static IEnumerator InitLocalization()
        {
            yield return LocalizationSettings.InitializationOperation;
        }
    }
}