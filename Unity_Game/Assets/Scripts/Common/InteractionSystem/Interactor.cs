using System.Linq;
using Common.DialogueSystem;
using Inventory;
using StarterAssets.ThirdPersonController.Scripts;
using UnityEngine;

namespace Common.InteractionSystem
{
    public class Interactor : MonoBehaviour, ICollector, ICarrier, ISpeak
    {
        private Interactable _currentInteractable; // interactable object that Interactor is looking at
        private Interactable _lastInteractable; // last saved object for calculations between checks/frames

        // ====== Setup ====== //
        [Header("Setup")] [SerializeField] private string targetTag = "Interactable";
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float rayMaxDistance = 5f;
        [SerializeField] private GameObject hint;
        [SerializeField] private ThirdPersonController controller;
        [SerializeField] private Animator animator;

        // ====== Interaction ====== //
        [Header("Interaction")] [SerializeField]
        private KeyCode defaultInteractKey = KeyCode.E;

        // ====== Inventory ====== //
        [Header("Inventory")] [SerializeField] private UI_Inventory uiInventory;
        public Inventory.Inventory inventory;

        [SerializeField] private KeyCode[] cancelInteractionKeys;

        // ====== Interactable Related State ====== //
        private KeyCode _interactKey;
        private string _taskHint;
        private InteractHint _interactHint;

        // ====== Carrier ====== //
        [Header("Carrier")] public GameObject box;
        private Interactable.InteractableObject _currentlyCarrying = Interactable.InteractableObject.None;

        // ====== Monologue ====== //
        private DialogueManager _dialogueManager;
        private bool _havingMonologue;
        
        // ====== State Booleans ====== //
        private bool _readyToInteract;
        private bool _interacting;
        private static readonly int HOLDING_BOX_IDLE = Animator.StringToHash("HoldingBoxIdle");
        private static readonly int HOLDING_BOX_WALK = Animator.StringToHash("HoldingBoxWalk");

        // Start is called before the first frame update
        private void Start()
        {
            inventory = new Inventory.Inventory();
            uiInventory.SetInventory(inventory);
            
            _dialogueManager = FindObjectOfType<DialogueManager>();
            if (_dialogueManager == null) Debug.LogError("DialogueManager is missing");

            _interactKey = defaultInteractKey;

            // Console warning if variables don't have a reference
            // if (interactorUI == null) Debug.LogWarning("Interactor: InteractorUI component was not set.");
            if (hint == null) Debug.LogWarning(this.name + ": Hint GameObject was not set.");
            else _interactHint = hint.GetComponent<InteractHint>();

            // Reset variables
            //interacting = false;
            // Call abort method to reset the variables
            AbortInteraction();
        }

        // Update is called once per frame
        private void Update()
        {
            _currentInteractable = null;

            // Debug.DrawRay(transform.position, ray.direction * rayMaxDistance, Color.green);

            if (!_interacting)
            {
                var ray = new Ray(transform.position, transform.forward);

                if (Physics.Raycast(ray, out var hit, rayMaxDistance, layerMask))
                {
                    if (hit.collider.CompareTag(targetTag))
                    {
                        Interactable interactable = hit.collider.GetComponent<Interactable>();
                        if (interactable != null) _currentInteractable = interactable;
                    }
                }

                // Check if the interactor was looking at an interactable object that it didn't last frame
                if (_currentInteractable != null && !_readyToInteract) CheckIfAvailableToInteract();
                else if (_currentInteractable == null && _readyToInteract) AbortInteraction();

                // Check if not interacting & ready to interact & pressed interact button -> interact
                if (_readyToInteract && Input.GetKeyDown(_interactKey)) Interact();
            }
            else if (IsCancelButtonPressed()) EndInteraction();

            if (_currentInteractable != null) _lastInteractable = _currentInteractable;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void CheckIfAvailableToInteract()
        {
            _readyToInteract = _currentInteractable.IsReadyToInteract(out _taskHint, out var interactKey);
            if (!_readyToInteract) return;

            // Set default interact key if KeyCode.None was provided
            _interactKey = interactKey == KeyCode.None ? defaultInteractKey : interactKey;

            _interactHint.SetHintMessage(CreateInteractMessage());
            hint.SetActive(true);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void AbortInteraction()
        {
            _interacting = false;
            _readyToInteract = false;
            hint.SetActive(false);
            // ReSharper disable once Unity.NoNullPropagation
            _lastInteractable?.OnAbortInteract();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Interact()
        {
            _interacting = true;
            _readyToInteract = false;
            hint.SetActive(false);

            switch (_currentInteractable)
            {
                case ISpeak:
                    animator.SetFloat("Speed", 0);
                    controller.enabled = false;
                    break;
                case IFixable fixable when fixable.CanBeFixed(inventory):
                    fixable.Fix(inventory);
                    break;
            }

            // ReSharper disable once Unity.NoNullPropagation
            _currentInteractable?.OnInteract(this);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void EndInteraction()
        {
            // setting readyInteract to false we allow for a clean check next frame/check
            // when an interacting is cancelled, a next suitable object could be ready to interact (already looking at it)
            // ...but without setting readyInteract to false, it thinks we aren't allowed to search for anything new
            // (will not call ReadyInteract method properly)

            if (_lastInteractable is ISpeak || _havingMonologue) controller.enabled = true;
            if (_havingMonologue)
            {
                _havingMonologue = false;
                _dialogueManager.Abort();
            }
            // ReSharper disable once Unity.NoNullPropagation
            _lastInteractable?.OnEndInteract();
            _interacting = false;
            _readyToInteract = false;
        }

        //pubic override method that calls EndInteract if the GameObject that requested matches lastInteractable
        public void EndInteraction(Interactable requester)
        {
            if (requester != _lastInteractable && requester is ISpeak)
            {
                Debug.LogError("Requester is an ISpeak interactable");
                return;
            }

            EndInteraction();
        }

        // Returns true if any of the "cancel" keys is pressed, else returns false
        private bool IsCancelButtonPressed()
        {
            // Checks if any button from the "cancel" keys was pressed down this frame
            return cancelInteractionKeys.Any(Input.GetKeyDown);
        }

        private string CreateInteractMessage()
        {
            return _taskHint.Replace("#", _interactKey.ToString());
        }

        public void Collect(Item.ItemType givenItemType)
        {
            // Add the collected item to the inventory
            inventory.AddItem(new Item { itemType = givenItemType, amount = 1 });
            Debug.Log("Collected: 1 " + givenItemType);
        }

        public void PickUp(Interactable.InteractableObject interactableObject)
        {
            switch (interactableObject)
            {
                case Interactable.InteractableObject.Box:
                    Debug.Log("Carry Box");
                    if (box == null)
                    {
                        Debug.LogError("Reference to Box does not exist");
                        return;
                    }

                    animator.SetBool(HOLDING_BOX_IDLE, true);
                    _currentlyCarrying = Interactable.InteractableObject.Box;
                    box.SetActive(true);
                    break;
                default:
                    return;
            }
        }

        public void Drop()
        {
            switch (_currentlyCarrying)
            {
                case Interactable.InteractableObject.Box:
                    Debug.Log("Drop Box");
                    animator.SetBool(HOLDING_BOX_WALK, false);
                    animator.SetBool(HOLDING_BOX_IDLE, false);
                    box.SetActive(false);
                    break;
                default:
                    return;
            }

            _currentlyCarrying = Interactable.InteractableObject.None;
        }

        public Interactable.InteractableObject GetCarryingObject()
        {
            return _currentlyCarrying;
        }

        public void TriggerMonologue(Dialogue dialogue)
        {
            _havingMonologue = true;
            _interacting = true;
            _readyToInteract = false;
            hint.SetActive(false);
            controller.enabled = false;
            
            _dialogueManager.StartDialogue(dialogue, this);
        }

        public void OnDialogueEnd(bool finished)
        {
            EndInteraction();
        }
    }
}