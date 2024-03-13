using System.Collections;
using Farm.Scripts.InteractionSystem;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public class QuestsHint : MonoBehaviour
{
    [SerializeField] private Interactable interactable;
    [SerializeField] private LocalizeStringEvent localizeStringEvent;
    [SerializeField] private string hintKey;

    private void Start()
    {
        // Wait for the localization to be initialized.
        StartCoroutine(InitLocalization());

        // Get the localized strings.
        const string table = "QuestsHintTable";

        // Quest hint.
        localizeStringEvent.StringReference.SetReference(table, hintKey);
        string hint = localizeStringEvent.StringReference.GetLocalizedString();

        // Set the hint.
        interactable.taskHint = hint;
    }

    private static IEnumerator InitLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;
    }
}