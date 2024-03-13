using Farm.Scripts.Interaction_System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

// Anything that we need to consider as Interactable needs to implement this class
public abstract class  Interactable : MonoBehaviour
{
    [Header("Interaction")] [SerializeField]
    protected string taskHint = "interact with something";

    [SerializeField]
    protected KeyCode interactKey;
    public bool readyToInteract = true;
    
    // Called when the Interactor wants to interact with the selected object
    public abstract void OnInteract(Interactor interactor);

    // Called when interaction has started and now ends (Forced or not).
    public abstract void OnEndInteract();

    // Called when object gets selected and ready to interact
    public abstract bool IsReadyToInteract([CanBeNull] out string taskHint, out KeyCode interactKey);

    // Called when object gets deselected from interacting
    public abstract void OnAbortInteract();
}