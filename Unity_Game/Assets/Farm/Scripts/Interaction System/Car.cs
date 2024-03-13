using System.Collections;
using System.Collections.Generic;
using Farm.Scripts.Interaction_System;
using UnityEngine;

public class Car : Interactable
{
    public GameObject exitCutScene;
    public override void OnInteract(Interactor interactor)
    {
        if (interactor == null) return;

        // Instant interaction
        Debug.Log("Interacted with car");
        exitCutScene.SetActive(true);
    }

    public override void OnEndInteract()
    {
        // TODO: Implement
    }

    public override bool IsReadyToInteract(out string taskHint, out KeyCode interactKey)
    {
        taskHint = this.taskHint;
        interactKey = this.interactKey;
        return readyToInteract;
    }

    public override void OnAbortInteract()
    {
        // TODO: Implement
    }
}
