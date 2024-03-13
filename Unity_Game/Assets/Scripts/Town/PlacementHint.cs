using System.Collections;
using Common.QuestSystem;
using Farm.Scripts.Interaction_System;
using Farm.Scripts.InteractionSystem;
using JetBrains.Annotations;
using UnityEngine;

namespace Town
{
    public class PlacementHint : Interactable
    {
        public float flashDuration = 1f;
        private Renderer myRenderer;
        public GameObject objectToActivate;

        public InteractableObject _interactableTargetObject; 

        private void Start()
        {
            myRenderer = GetComponent<Renderer>();
            // Start the flashing coroutine when the script is enabled
            StartCoroutine(FlashHint());
        }

        private IEnumerator FlashHint()
        {
            float elapsedTime = 0f;
            while (true)
            {
                float lerpValue = Mathf.PingPong(elapsedTime / flashDuration, 1f);
                myRenderer.material.color = new Color(myRenderer.material.color.r, myRenderer.material.color.g, myRenderer.material.color.b, lerpValue);

                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }

        // You can stop the flashing by calling this method
        public void StopFlashing()
        {
            StopAllCoroutines();
            // Ensure the renderer is fully opaque when stopped
            myRenderer.material.color = new Color(myRenderer.material.color.r, myRenderer.material.color.g, myRenderer.material.color.b, 1f);
        }

        public override void OnInteract(Interactor interactor)
        {
            base.OnInteract(interactor);

            if (interactor.GetCarryingObject() == _interactableTargetObject)
            {
                // Instant interaction
                interactor.Drop();
                objectToActivate.SetActive(true);
                gameObject.SetActive(false);
                
                if (QuestManager.Instance.currentQuest is CarryQuest quest)
                {
                    quest.OnObjectPlaced(_interactableTargetObject);
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