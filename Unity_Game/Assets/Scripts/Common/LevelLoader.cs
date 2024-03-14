using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
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
}