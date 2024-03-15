using System.Collections;
using System.Collections.Generic;
using Common.DialogueSystem;
using Common.QuestSystem;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Forest.Quests.Find_Book_Quest
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
            const string table = "FindTheBookQuestTable";

            // Quest title.
            localizeStringEvent.StringReference.SetReference(table, "title");
            string title = localizeStringEvent.StringReference.GetLocalizedString();

            // Quest description.
            localizeStringEvent.StringReference.SetReference(table, "description");
            string description = localizeStringEvent.StringReference.GetLocalizedString();

            // Intro dialogues.
            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_alex_1");
            string introDialogueAlex1 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_samir_1");
            string introDialogueSamir1 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_samir_2");
            string introDialogueSamir2 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "intro_dialogue_alex_2");
            string introDialogueAlex2 = localizeStringEvent.StringReference.GetLocalizedString();

            // In progress dialogue.
            localizeStringEvent.StringReference.SetReference(table, "in_progress_dialogue_samir");
            string inProgressDialogueSamir = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "in_progress_dialogue_alex");
            string inProgressDialogueAlex = localizeStringEvent.StringReference.GetLocalizedString();

            // On completion dialogues.
            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_alex_1");
            string onCompletionDialogueAlex1 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_samir_1");
            string onCompletionDialogueSamir1 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_alex_2");
            string onCompletionDialogueAlex2 = localizeStringEvent.StringReference.GetLocalizedString();

            localizeStringEvent.StringReference.SetReference(table, "on_completion_dialogue_samir_2");
            string onCompletionDialogueSamir2 = localizeStringEvent.StringReference.GetLocalizedString();

            // Set the localized strings to the quest.
            // Set the title and description.
            quest.title = title;
            quest.description = description;

            // Clear the dialogue sentences.
            quest.introDialogue.sentences.Clear();
            quest.inProgressDialogue.sentences.Clear();
            quest.onCompletionDialogue.sentences.Clear();

            // Set the intro dialogues.
            quest.introDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new("Alex", introDialogueAlex1),
                new("???", introDialogueSamir1),
                new("Alex", introDialogueAlex2),
                new("Samir", introDialogueSamir2)
            });

            // Set the in progress dialogues.
            quest.inProgressDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new("Samir", inProgressDialogueSamir),
                new("Alex", inProgressDialogueAlex)
            });

            // Set the on completion dialogues.
            quest.onCompletionDialogue.sentences.AddRange(new List<DialogueSentence>
            {
                new("Alex", onCompletionDialogueAlex1),
                new("Samir", onCompletionDialogueSamir1),
                new("Alex", onCompletionDialogueAlex2),
                new("Samir", onCompletionDialogueSamir2)
            });

            // Update the quest UI.
            QuestManager.instance.UpdateQuestUI();
        }

        private static IEnumerator InitLocalization()
        {
            yield return LocalizationSettings.InitializationOperation;
        }
    }
}