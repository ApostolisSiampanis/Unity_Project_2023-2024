using System;
using Farm.Scripts.DialogueSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace Farm.Scripts.Interaction_System
{
    public class NPC : MonoBehaviour, Interactable, ISpeak
    {
        // ====== INTERACTION ====== //
        [Header("Interaction")] [SerializeField]
        private string _taskHint = "talk to Bob";

        [SerializeField] private KeyCode _interactKey;
        [SerializeField] private bool _readyToInteract = true;

        private Interactor _interactor;
        private DialogueTrigger _dialogueTrigger;

        private Animator _animator;
        private NavMeshAgent _navMeshAgent;

        private Quaternion _prevRotation;

        private QuestManager _questManager;
        public Quest availableQuest;

        private bool _isTalking;

        public void Start()
        {
            _isTalking = false;

            _dialogueTrigger = GetComponent<DialogueTrigger>();
            _animator = GetComponent<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _questManager = QuestManager.Instance;

            // Check if necessary objects are present
            if (_dialogueTrigger == null) Debug.LogError("DialogueTrigger is missing");
            if (_animator == null) Debug.LogError("Animator is missing");
            if (_navMeshAgent == null) Debug.LogError("NavMeshAgent is missing");
            if (_questManager == null) Debug.LogError("QuestManager is missing");
        }

        public void OnInteract(Interactor interactor)
        {
            _interactor = interactor;

            if (_dialogueTrigger == null) return;

            _isTalking = true;
            ChangeState();
            
            if (availableQuest != null)
            {
                switch (availableQuest.state)
                {
                    case Quest.State.NotStarted:
                        _dialogueTrigger.TriggerDialogue(availableQuest.introDialogue, this);
                        break;
                    case Quest.State.InProgress:
                        _dialogueTrigger.TriggerDialogue(availableQuest.inProgressDialogue, this);
                        break;
                    case Quest.State.Completed:
                        _dialogueTrigger.TriggerDialogue(availableQuest.onCompletionDialogue, this);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                _dialogueTrigger.Greet(this);
            }
            
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
            if (!_isTalking) return;
            
            if (wasFinished && availableQuest != null)
            {
                switch (availableQuest.state)
                {
                    case Quest.State.NotStarted:
                        _questManager.AcceptQuest(this);
                        break;
                    case Quest.State.Completed:
                        _questManager.CompleteQuest(_interactor, this);
                        break;
                    case Quest.State.InProgress:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            _interactor.EndInteraction(this);
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