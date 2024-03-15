using System.Collections;
using System.Collections.Generic;
using Common.DialogueSystem;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Farm.Quests
{
    public class InteractWithSprinkler : MonoBehaviour
    {
        [SerializeField] private LocalizeStringEvent localizeStringEvent;

        public Dialogue GetMonologue()
        {
            // Wait for the localization to be initialized.
            StartCoroutine(InitLocalization());

            // Get the localized strings.
            const string table = "WaterTheCropsQuestDialoguesTable";

            // Create Dialogue instance with the localized string.
            localizeStringEvent.StringReference.SetReference(table, "in_progress_monologue_alex");
            Dialogue dialogue = new()
            {
                sentences = new List<DialogueSentence>
                {
                    new("Alex",
                        localizeStringEvent.StringReference.GetLocalizedString())
                }
            };

            // Return the Dialogue instance.
            return dialogue;
        }

        private static IEnumerator InitLocalization()
        {
            yield return LocalizationSettings.InitializationOperation;
        }
    }
}