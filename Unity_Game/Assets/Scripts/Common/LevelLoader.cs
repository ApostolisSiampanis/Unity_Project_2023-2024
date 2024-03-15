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
        public enum Scene
        {
            MainMenu,
            Farm,
            Forest,
            Town
        }
        
        public Interactor interactor;
        public GameObject player;
        
        // Transition animation
        public Animator transition;
        public float transitionTime = 1f;

        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadScreen(sceneIndex));
        }

        protected IEnumerator LoadScreen(int sceneIndex)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSeconds(transitionTime);
            SceneManager.LoadScene(sceneIndex);
        }

        public abstract void InitScene();
    }
}