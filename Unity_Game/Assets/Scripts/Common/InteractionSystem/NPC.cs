using System;
using Common.DialogueSystem;
using Common.QuestSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace Common.InteractionSystem
{
    public class NPC : Interactable, ISpeak
    {
        protected Interactor Interactor;
        private DialogueTrigger _dialogueTrigger;

        protected Animator Animator;
        protected NavMeshAgent NavMeshAgent;

        protected Quaternion PrevRotation;

        public GameObject questHint;
        public Quest availableQuest;

        protected bool IsTalking;
        private static readonly int IS_WALKING = Animator.StringToHash("isWalking");

        public void Start()
        {
            IsTalking = false;

            _dialogueTrigger = GetComponent<DialogueTrigger>();
            Animator = GetComponent<Animator>();
            NavMeshAgent = GetComponent<NavMeshAgent>();

            // Check if necessary objects are present
            if (_dialogueTrigger == null) Debug.LogError("DialogueTrigger is missing");
            if (Animator == null) Debug.LogError("Animator is missing");
            if (NavMeshAgent == null) Debug.LogError("NavMeshAgent is missing");
        }

        public override void OnInteract(Interactor interactor)
        {
            Interactor = interactor;
            base.OnInteract(interactor);

            if (_dialogueTrigger == null) return;

            IsTalking = true;
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
            if (IsTalking)
            {
                IsTalking = false;
                _dialogueTrigger.Abort();
                Debug.Log("Abort dialogue.");
            }


            Debug.Log("End of interaction.");
            ChangeState();
        }

        public override bool IsReadyToInteract(out string taskHint, out KeyCode interactKey)
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
            if (!IsTalking) return;

            if (wasFinished && availableQuest != null)
            {
                switch (availableQuest.state)
                {
                    case Quest.State.NotStarted:
                        availableQuest.StartQuest(this);
                        break;
                    case Quest.State.Completed:
                        availableQuest.CompleteQuest(Interactor, this);
                        break;
                    case Quest.State.InProgress:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Interactor.EndInteraction(this);
        }

        public void ShowQuestHint(bool show)
        {
            questHint.SetActive(show);
        }

        protected virtual void ChangeState()
        {
            Animator.SetBool(IS_WALKING, !IsTalking);
            NavMeshAgent.isStopped = IsTalking;

            if (IsTalking)
            {
                // Change orientation to look at the interactor
                PrevRotation = transform.rotation;
                var direction = Interactor.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                // Change the orientation back to normal
                transform.rotation = PrevRotation;
            }
        }
    }
}