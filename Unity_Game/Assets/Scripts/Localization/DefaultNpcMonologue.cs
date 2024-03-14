using System.Collections;
using Common.DialogueSystem;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Localization
{
    public class DefaultNpcMonologue : MonoBehaviour
    {
        [SerializeField] private DialogueTrigger dialogueTrigger;
        [SerializeField] private LocalizeStringEvent localizeStringEvent;
        [SerializeField] private string npcName;
        [SerializeField] private string defaultNpcMonologueKey;

        private void Start()
        {
            // Wait for the localization to be initialized.
            StartCoroutine(InitLocalization());

            // Get the localized strings.
            const string table = "DefaultNpcMonologueTable";

            // Default NPC monologue.
            localizeStringEvent.StringReference.SetReference(table, defaultNpcMonologueKey);
            string monologue = localizeStringEvent.StringReference.GetLocalizedString();

            // Remove the previous default monologue.
            dialogueTrigger.greetDialogue.sentences.Clear();

            // Set the default monologue.
            dialogueTrigger.greetDialogue.sentences.Add(new DialogueSentence(npcName, monologue));
        }

        private static IEnumerator InitLocalization()
        {
            yield return LocalizationSettings.InitializationOperation;
        }
    }
}