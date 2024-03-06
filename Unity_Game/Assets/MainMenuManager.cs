using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject optionsScreen;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Resolutions")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;

    // Temporary variables to store changes
    private float tempVolume;
    private int tempQualityIndex;
    private bool tempIsFullscreen;
    private int tempResolutionIndex;

    // Start is called before the first frame update
    private void Start()
    {
        // Get the resolution dropdown specified for this computer
        resolutions = Screen.resolutions;
        //resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Initially set the main menu screen to be active
        mainMenuScreen.SetActive(true);
        optionsScreen.SetActive(false);
    }

    // Main Menu Screen
    public void PlayGame()
    {
        SceneManager.LoadScene("FarmScene");
    }

    public void Options()
    {
        // Set the main menu screen to be inactive and the options screen to be active
        mainMenuScreen.SetActive(false);
        optionsScreen.SetActive(true);

        // Set the temporary variables to the current settings
        audioMixer.GetFloat("volume", out tempVolume);
        tempQualityIndex = QualitySettings.GetQualityLevel();
        tempIsFullscreen = Screen.fullScreen;
        tempResolutionIndex = GetResolutionIndex(Screen.currentResolution);
    }

    private int GetResolutionIndex(Resolution resolution)
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == resolution.width && resolutions[i].height == resolution.height)
            {
                return i;
            }
        }
        return 0;
    }

    public void Quit()
    {
        Application.Quit();
    }

    // Options Screen
    public void SetVolume(float volume)
    {
        tempVolume = volume;
    }

    public void SetQuality(int qualityIndex)
    {
        tempQualityIndex = qualityIndex;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        tempIsFullscreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        tempResolutionIndex = resolutionIndex;
    }

    public void SaveSettings()
    {
        // Apply settings changes
        audioMixer.SetFloat("volume", tempVolume);
        QualitySettings.SetQualityLevel(tempQualityIndex);
        Screen.fullScreen = tempIsFullscreen;
        Resolution resolution = resolutions[tempResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        // Set screens to be active/inactive
        mainMenuScreen.SetActive(true);
        optionsScreen.SetActive(false);
    }

    public void CancelSettings()
    {
        // Set screens to be active/inactive
        mainMenuScreen.SetActive(true);
        optionsScreen.SetActive(false);
    }
}
