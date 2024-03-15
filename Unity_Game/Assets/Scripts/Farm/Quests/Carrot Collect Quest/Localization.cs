using System.Collections;
using System.Collections.Generic;
using Common.DialogueSystem;
using Common.QuestSystem;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Farm.Quests.Carrot_Collect_Quest
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
            const string table = "CarrotCollectQuestDialoguesTable";

            // Quest title.
            localizeStringEvent.StringReference.SetReference(table, "title");
            string title = localizeStringEvent.StringReference.GetLocalizedString();

            // Quest description.
            localizeStringEvent.StringReference.SetReference(table, "description");
            string description = localizeStringEvent.StringReference.GetLocalizedString();

            // Intro dialogue.
            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_grandpa");
            string introDialogueGrandpa = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_alex");
            string introDialogueAlex = localizeStringEvent.StringReference.GetLocalizedString();

            // In progress grandpa monologue.
            localizeStringEvent.StringReference.SetReference(table, "in_progress_monologue_grandpa");
            string inProgressMonologueGrandpa = localizeStringEvent.StringReference.GetLocalizedString();

            // On completion dialogue.
            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_grandpa");
            string onCompletionDialogueGrandpa = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_alex");
            string onCompletionDialogueAlex = localizeStringEvent.StringReference.GetLocalizedString();

            // Clear the dialogue sentences.
            quest.introDialogue.sentences.Clear();
            quest.inProgressDialogue.sentences.Clear();
            quest.onCompletionDialogue.sentences.Clear();

            // Set the intro dialogue.
            quest.introDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new("Grandpa", introDialogueGrandpa),
                new("Alex", introDialogueAlex)
            });

            // Set the in progress monologue.
            quest.inProgressDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new("Grandpa", inProgressMonologueGrandpa)
            });

            // Set the on completion dialogue.
            quest.onCompletionDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new("Grandpa", onCompletionDialogueGrandpa),
                new("Alex", onCompletionDialogueAlex)
            });

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