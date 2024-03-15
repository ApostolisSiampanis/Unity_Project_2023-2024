using System.Collections;
using System.Collections.Generic;
using Common.DialogueSystem;
using Common.QuestSystem;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Farm.Quests.Water_Crops_Quest
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
            const string table = "WaterTheCropsQuestDialoguesTable";

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

            // In progress alex monologue.
            localizeStringEvent.StringReference.SetReference(table, "in_progress_monologue_alex");
            string inProgressMonologueAlex = localizeStringEvent.StringReference.GetLocalizedString();

            // On completion dialogues.
            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_alex_1");
            string onCompletionDialogueAlex1 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_grandpa_1");
            string onCompletionDialogueGrandpa1 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_alex_2");
            string onCompletionDialogueAlex2 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_grandpa_2");
            string onCompletionDialogueGrandpa2 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_alex_3");
            string onCompletionDialogueAlex3 = localizeStringEvent.StringReference.GetLocalizedString();

            // Set the localized strings to the quest.
            // Set the title and description.
            quest.title = title;
            quest.description = description;

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

            // Set the in progress monologues.
            quest.inProgressDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new("Grandpa", inProgressMonologueGrandpa),
                new("Alex", inProgressMonologueAlex)
            });

            // Set the on completion dialogues.
            quest.onCompletionDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new("Alex", onCompletionDialogueAlex1),
                new("Grandpa", onCompletionDialogueGrandpa1),
                new("Alex", onCompletionDialogueAlex2),
                new("Grandpa", onCompletionDialogueGrandpa2),
                new("Alex", onCompletionDialogueAlex3)
            });
        }

        private static IEnumerator InitLocalization()
        {
            yield return LocalizationSettings.InitializationOperation;
        }
    }
}