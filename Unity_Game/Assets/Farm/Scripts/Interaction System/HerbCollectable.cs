using UnityEngine;

namespace Farm.Scripts.Interaction_System
{
    public class HerbCollectable : MonoBehaviour, Interactable
    {
        [Header("Interaction")]
        [SerializeField] private string _taskHint = "collect herb";
        [SerializeField] private KeyCode _interactKey;
        [SerializeField] private bool _readyToInteract = true;

        public TaskStatus Status { get; set; }

        public void Start()
        {
            Status = TaskStatus.NOT_STARTED;
        }

        public void OnInteract(Interactor interactor)
        {
            Status = TaskStatus.IN_PROCESS;
            gameObject.SetActive(false);
            Status = TaskStatus.COMPLETED;
            interactor.EndInteraction(this);
        }

        public TaskStatus OnEndInteract()
        {
            if (Status == TaskStatus.IN_PROCESS) Status = TaskStatus.ABORTED;
            return Status;
        }

        public bool IsReadyToInteract(out string taskHint, out KeyCode interactKey)
        {
            taskHint = _taskHint;
            interactKey = _interactKey;
            return _readyToInteract;
        }

        public void OnAbortInteract()
        {
            throw new System.NotImplementedException();
        }
    }
}
