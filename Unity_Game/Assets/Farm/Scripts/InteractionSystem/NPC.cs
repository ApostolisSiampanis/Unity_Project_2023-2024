using System;
using Farm.Scripts.DialogueSystem;
using Farm.Scripts.InteractionSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace Farm.Scripts.Interaction_System
{
    public class NPC : Interactable, ISpeak
    {
        private Interactor _interactor;
        private DialogueTrigger _dialogueTrigger;

        private Animator _animator;
        private NavMeshAgent _navMeshAgent;

        private Quaternion _prevRotation;
        
        public GameObject questHint;
        public Quest availableQuest;

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

        public override void OnInteract(Interactor interactor)
        {
            _interactor = interactor;
            base.OnInteract(interactor);

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

        public override void OnEndInteract()
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

        public override bool IsReadyToInteract([CanBeNull] out string taskHint, out KeyCode interactKey)
        {
            taskHint = base.taskHint;
            interactKey = base.interactKey;

            return readyToInteract;
        }

        public override void OnAbortInteract()
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
                        availableQuest.StartQuest(this);
                        break;
                    case Quest.State.Completed:
                        availableQuest.CompleteQuest(_interactor, this);
                        break;
                    case Quest.State.InProgress:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            _interactor.EndInteraction(this);
        }

        public void ShowQuestHint(bool show)
        {
            questHint.SetActive(show);
        }

        protected virtual void ChangeState()
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