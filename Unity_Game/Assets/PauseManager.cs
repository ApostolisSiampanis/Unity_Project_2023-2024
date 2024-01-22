using StarterAssets.ThirdPersonController.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private GameObject pauseScreen;

    [Header("Things to Disable")]
    [SerializeField] private ThirdPersonController controller;

    bool isPaused = false;

    bool cursorVisibility;
    CursorLockMode cursorState;

    // Start is called before the first frame update
    void Start()
    {
        isPaused = true;
        unPause();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (!isPaused)
            {
                Pause();
            }  
            else
            {
                unPause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;

        cursorVisibility = Cursor.visible;
        cursorState = Cursor.lockState;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // disable player movement
        controller.enabled = false;
        // pause
        pauseScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void unPause()
    {
        isPaused = false;

        Cursor.visible = cursorVisibility;
        Cursor.lockState = cursorState;

        // unpause
        pauseScreen.SetActive(false);
        Time.timeScale = 1f;
        // enable player movement
        controller.enabled = true;
    }

    public void Menu()
    {
        Debug.Log("Menu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
