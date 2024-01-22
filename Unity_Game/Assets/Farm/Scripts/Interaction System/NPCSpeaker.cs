using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Farm.Scripts.Interaction_System;
using JetBrains.Annotations;
using UnityEngine;

public class NPCSpeaker : MonoBehaviour, Interactable
{

    [Header("Interaction")]
    [SerializeField] private string _taskHint = "talk to Bob";
    [SerializeField] private KeyCode _interactKey;
    [SerializeField] private bool _readyToInteract = true;
    
    private Interactor _interactor;
    
    public TaskStatus Status { get; set; }
    
    public void Start()
    {
        Status = TaskStatus.NOT_STARTED;
    }

    public void OnInteract(Interactor interactor)
    {
        _interactor = interactor;
        Status = TaskStatus.IN_PROCESS;
        // 
        // Status = TaskStatus.COMPLETED;
        // _interactor.EndInteraction(this);
    }

    // TODO: Change return type to boolean (or TaskStatus) for task complete status
    public TaskStatus OnEndInteract()
    {
        if (Status == TaskStatus.IN_PROCESS) Status = TaskStatus.ABORTED;
        Debug.Log("End of interaction.");
        return Status;
    }

    public bool IsReadyToInteract([CanBeNull] out string taskHint, out KeyCode interactKey)
    {
        taskHint = _taskHint;
        interactKey = _interactKey;

        return _readyToInteract;
    }

    public void OnAbortInteract()
    {
        Debug.Log("Abort interact");
    }
}
