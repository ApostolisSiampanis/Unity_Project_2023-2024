using System.Linq;
using StarterAssets.ThirdPersonController.Scripts;
using UnityEngine;

namespace Farm.Scripts.Interaction_System
{
    public class Interactor : MonoBehaviour, ICollector
    {
        private Interactable _currentInteractable; // interactable object that Interactor is looking at
        private Interactable _lastInteractable; // last saved object for calculations between checks/frames

        // ====== SETUP ====== //
        [Header("Setup")] [SerializeField] private string targetTag = "Interactable";
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float rayMaxDistance = 5f;
        [SerializeField] private GameObject hint;
        [SerializeField] private ThirdPersonController controller;
        [SerializeField] private Animator animator;

        // ====== INTERACTION ====== //
        [Header("Interaction")] [SerializeField]
        private KeyCode defaultInteractKey = KeyCode.E;

        // ====== INVENTORY ====== //
        [Header("Inventory")] [SerializeField] private UI_Inventory uiInventory;
        private Inventory _inventory;

        [SerializeField] private KeyCode[] cancelInteractionKeys;

        // ====== Interactable Related State ====== //
        private KeyCode _interactKey;
        private string _taskHint;
        private InteractHint _interactHint;

        // ====== State Booleans ====== //
        private bool _readyToInteract;
        private bool _interacting;

        // Start is called before the first frame update
        private void Start()
        {
            _inventory = new Inventory();
            uiInventory.SetInventory(_inventory);

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
            else if (_interacting && IsCancelButtonPressed()) EndInteraction();

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
            _lastInteractable?.OnAbortInteract();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Interact()
        {
            _interacting = true;
            _readyToInteract = false;
            hint.SetActive(false);

            if (_currentInteractable is ISpeak)
            {
                animator.SetFloat("Speed", 0);
                controller.enabled = false;
            }

            _currentInteractable?.OnInteract(this);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void EndInteraction()
        {
            // setting readyInteract to false we allow for a clean check next frame/check
            // when an interacting is cancelled, a next suitable object could be ready to interact (already looking at it)
            // ...but without setting readyInteract to false, it thinks we aren't allowed to search for anything new
            // (will not call ReadyInteract method properly)
            
            if (_lastInteractable is ISpeak) controller.enabled = true;
            _lastInteractable.OnEndInteract();
            _interacting = false;
            _readyToInteract = false;
        }

        //pubic override method that calls EndInteract if the GameObject that requested matches lastInteractable
        public void EndInteraction(Interactable requester)
        {
            // TODO:
            if (requester != _lastInteractable && requester is not Collectable)
            {
                Debug.LogError("Requester is not last interactable");
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
            if (_taskHint == null) return "Press [" + _interactKey + "] to Interact";
            return "Press [" + _interactKey + "] to " + _taskHint;
        }

        public void Collect(Item.ItemType givenItemType)
        {
            // Add the collected item to the inventory
            _inventory.AddItem(new Item { itemType = givenItemType, amount = 1 });
            Debug.Log("Collected: 1 " + givenItemType);
        }
    }
}