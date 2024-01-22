using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractHint : MonoBehaviour
{
    public string _hintMessage = "Press E to Interact";
    private TextMeshProUGUI textMesh;

    public void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null) Debug.LogWarning(name + ": Text Mesh Pro not found.");
    }

    public void SetHintMessage(string hintMessage)
    {
        _hintMessage = hintMessage;
        if (textMesh != null) textMesh.text = _hintMessage;
    }
    
}
