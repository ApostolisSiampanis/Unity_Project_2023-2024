using System.Collections;
using System.Collections.Generic;
using Common.DialogueSystem;
using Common.QuestSystem;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Town.Quests.Truck_Unloading_Quest
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
            const string table = "TruckUnloadingQuestDialoguesTable";

            // Quest title.
            localizeStringEvent.StringReference.SetReference(table, "title");
            string title = localizeStringEvent.StringReference.GetLocalizedString();

            // Quest description.
            localizeStringEvent.StringReference.SetReference(table, "description");
            string description = localizeStringEvent.StringReference.GetLocalizedString();

            // Intro dialogues.
            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_uncle_1");
            string introDialogueUncle1 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_alex_1");
            string introDialogueAlex1 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_uncle_2");
            string introDialogueUncle2 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_alex_2");
            string introDialogueAlex2 = localizeStringEvent.StringReference.GetLocalizedString();

            // In progress uncle monologue.
            localizeStringEvent.StringReference.SetReference(table, "in_progress_monologue_uncle");
            string inProgressMonologueUncle = localizeStringEvent.StringReference.GetLocalizedString();

            // On completion dialogue.
            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_uncle");
            string onCompletionDialogueUncle = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_alex");
            string onCompletionDialogueAlex = localizeStringEvent.StringReference.GetLocalizedString();

            // Set the localized strings to the quest.
            // Set the title and description.
            quest.title = title;
            quest.description = description;

            // Clear the dialogue sentences.
            quest.introDialogue.sentences.Clear();
            quest.inProgressDialogue.sentences.Clear();
            quest.onCompletionDialogue.sentences.Clear();

            // Check if the selected locale is English, to set the correct name for Uncle Bob.
            bool isEnglish = LocalizationSettings.SelectedLocale.name == "en";
            string uncle = isEnglish ? "Uncle Bob" : "Θείος Bob";

            // Set the intro dialogue.
            quest.introDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new(uncle, introDialogueUncle1),
                new("Alex", introDialogueAlex1),
                new(uncle, introDialogueUncle2),
                new("Alex", introDialogueAlex2)
            });

            // Set the in progress monologue.
            quest.inProgressDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new(uncle, inProgressMonologueUncle)
            });

            // Set the on completion dialogue.
            quest.onCompletionDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new(uncle, onCompletionDialogueUncle),
                new("Alex", onCompletionDialogueAlex)
            });
        }

        private static IEnumerator InitLocalization()
        {
            yield return LocalizationSettings.InitializationOperation;
        }
    }
}