using Farm.Scripts.DialogueSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace Farm.Scripts.Interaction_System
{
    public class NPCSpeaker : MonoBehaviour, Interactable, ISpeak
    {
        // ====== INTERACTION ====== //
        [Header("Interaction")]
        [SerializeField] private string _taskHint = "talk to Bob";
        [SerializeField] private KeyCode _interactKey;
        [SerializeField] private bool _readyToInteract = true;

        private Interactor _interactor;
        private DialogueTrigger _dialogueTrigger;
        
        private Animator _animator;
        private NavMeshAgent _navMeshAgent;

        private Quaternion _prevRotation;

        private bool _isTalking;

        public void Start()
        {
            _isTalking = false;
            
            _dialogueTrigger = GetComponent<DialogueTrigger>();
            _animator = GetComponent<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            
            // Check if necessary objects are present
            if (_dialogueTrigger == null) Debug.LogError("DialogueTrigger is missing");
            if (_animator == null) Debug.LogError("Animator is missing");
            if (_navMeshAgent == null) Debug.LogError("NavMeshAgent is missing");
            
        }

        public void OnInteract(Interactor interactor)
        {
            _interactor = interactor;

            if (_dialogueTrigger == null) return;
            
            _isTalking = true;
            ChangeState();
            _dialogueTrigger.TriggerDialogue(this);
            
        }
        
        public void OnEndInteract()
        {
            if (_isTalking)
            {
                _isTalking = false;
                _dialogueTrigger.Abort();
                Debug.Log("Abort dialogue.");
            }
            Debug.Log("End of interaction.");
            ChangeState();
        }

        public bool IsReadyToInteract([CanBeNull] out string taskHint, out KeyCode interactKey)
        {
            taskHint = _taskHint;
            interactKey = _interactKey;

            return _readyToInteract;
        }

        public void OnAbortInteract()
        {
            Debug.Log("Abort interaction");
        }

        public void OnDialogueEnd(bool wasFinished)
        {
            if (!_isTalking || !wasFinished) return;
            _interactor.EndInteraction(this);
            Debug.Log("Dialog finished successfully.");
        }

        private void ChangeState()
        {
            _animator.SetBool("isWalking", !_isTalking);
            _navMeshAgent.isStopped = _isTalking;

            if (_isTalking)
            {
                // Change orientation to look at the interactor
                _prevRotation = transform.rotation;
                var direction = _interactor.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                // Change the orientation back to normal
                transform.rotation = _prevRotation;
            }
        }
    }
}
