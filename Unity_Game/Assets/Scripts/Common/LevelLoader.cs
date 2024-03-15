using System;
using System.Collections;
using Common.InteractionSystem;
using Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
    public abstract class LevelLoader : MonoBehaviour
    {
        public Interactor interactor;
        public GameObject player;
        
        // Transition animation
        public Animator transition;
        public float transitionTime = 1f;

        public void LoadNextScene()
        {
            StartCoroutine(LoadScreen(SceneManager.GetActiveScene().buildIndex + 1));
        }

        IEnumerator LoadScreen(int sceneIndex)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSeconds(transitionTime);
            SceneManager.LoadScene(sceneIndex);
        }

        protected abstract void InitScene();
    }
}