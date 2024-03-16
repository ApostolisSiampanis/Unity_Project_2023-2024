using System.Collections;
using Common.InteractionSystem;
using Common.QuestSystem;
using UnityEngine;

namespace Town
{
    public class PlacementHint : Interactable
    {
        public float flashDuration = 1f;
        private Renderer _myRenderer;
        public GameObject objectToActivate;

        public InteractableObject interactableTargetObject;

        private void Start()
        {
            _myRenderer = GetComponent<Renderer>();
            // Start the flashing coroutine when the script is enabled
            StartCoroutine(FlashHint());
        }

        private IEnumerator FlashHint()
        {
            float elapsedTime = 0f;
            while (true)
            {
                float lerpValue = Mathf.PingPong(elapsedTime / flashDuration, 1f);
                _myRenderer.material.color = new Color(_myRenderer.material.color.r, _myRenderer.material.color.g,
                    _myRenderer.material.color.b, lerpValue);

                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }

        // You can stop the flashing by calling this method
        public void StopFlashing()
        {
            StopAllCoroutines();
            // Ensure the renderer is fully opaque when stopped
            _myRenderer.material.color = new Color(_myRenderer.material.color.r, _myRenderer.material.color.g,
                _myRenderer.material.color.b, 1f);
        }

        public override void OnInteract(Interactor interactor)
        {
            base.OnInteract(interactor);

            if (interactor.GetCarryingObject() == interactableTargetObject)
            {
                // Instant interaction
                interactor.Drop();
                objectToActivate.SetActive(true);
                gameObject.SetActive(false);

                if (QuestManager.instance.currentQuest is CarryQuest quest)
                {
                    quest.OnObjectPlaced(interactableTargetObject);
                }
            }

            interactor.EndInteraction(this);
        }

        public override void OnEndInteract()
        {
            // Do nothing
        }

        public override void OnAbortInteract()
        {
            // Do nothing
        }
    }
}