using System.Collections.Generic;
using Save;
using Save.Data;
using StarterAssets.ThirdPersonController.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject optionsScreen;
    [SerializeField] private GameObject helpScreen;

    [Header("Things to Disable")] [SerializeField]
    private ThirdPersonController controller;

    [Header("Audio")] [SerializeField] private AudioMixer mainMixer;
    
    [Header("UI Elements")] 
    public Slider soundsVolume;
    public Slider musicVolume;
    public Toggle fullScreenToggle;
    public TMP_Dropdown qualityDropDown;
    public TMP_Dropdown resolutionDropdown;

    bool isPaused = false;

    bool cursorVisibility;
    CursorLockMode cursorState;

    Resolution[] resolutions;

    // Temporary variables to store changes
    private float _tempSoundsVolume;
    private float _tempMusicVolume;
    private int _tempQualityIndex;
    private bool _tempIsFullscreen;
    private int _tempResolutionIndex;

    private float _previousSoundsVolume;

    // Start is called before the first frame update
    void Start()
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
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        
        var settings = SaveSystem.LoadSettings();
        if (settings != null)
        {
            _tempSoundsVolume = settings.soundsVolume;
            _tempMusicVolume = settings.musicVolume;
            _tempQualityIndex = settings.qualityIndex;
            _tempIsFullscreen = settings.isFullscreen;
            _tempResolutionIndex = settings.resolutionIndex;

            soundsVolume.value = _tempSoundsVolume;
            musicVolume.value = _tempMusicVolume;
            fullScreenToggle.isOn = _tempIsFullscreen;
            qualityDropDown.value = _tempQualityIndex;
            resolutionDropdown.value = _tempResolutionIndex;

            UpdateSettings();
        }

        // Set the pause screen to be active
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

    //Pause Screen
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
        optionsScreen.SetActive(false);
        helpScreen.SetActive(false);
        Time.timeScale = 0f;

        // keep track of the previous volume
        mainMixer.GetFloat("SoundsParam", out _previousSoundsVolume);
        // Set volume to -80db when paused
        mainMixer.SetFloat("SoundsParam", -80f);
        mainMixer.SetFloat("MusicParam", _tempMusicVolume);
    }

    public void unPause()
    {
        isPaused = false;

        Cursor.visible = cursorVisibility;
        Cursor.lockState = cursorState;

        // unpause
        pauseScreen.SetActive(false);
        optionsScreen.SetActive(false);
        helpScreen.SetActive(false);
        Time.timeScale = 1f;
        // enable player movement
        controller.enabled = true;

        // Restore volume to its previous value when unpaused
        mainMixer.SetFloat("SoundsParam", _previousSoundsVolume);
        mainMixer.SetFloat("MusicParam", -80f);
    }

    public void Options()
    {
        pauseScreen.SetActive(false);
        optionsScreen.SetActive(true);
        helpScreen.SetActive(false);

        // Set the temporary variables to the current settings
        mainMixer.GetFloat("SoundsParam", out _tempSoundsVolume);
        mainMixer.GetFloat("MusicParam", out _tempMusicVolume);
        _tempQualityIndex = QualitySettings.GetQualityLevel();
        _tempIsFullscreen = Screen.fullScreen;
        _tempResolutionIndex = GetResolutionIndex(Screen.currentResolution);
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

    //Options Screen
    public void SetSoundsVolume(float volume)
    {
        _tempSoundsVolume = volume;
        _previousSoundsVolume = _tempSoundsVolume;
    }

    public void SetMusicVolume(float volume)
    {
        _tempMusicVolume = volume;
    }

    public void SetQuality(int qualityIndex)
    {
        _tempQualityIndex = qualityIndex;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        _tempIsFullscreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        _tempResolutionIndex = resolutionIndex;
    }

    public void SaveSettings()
    {
        UpdateSettings();

        // Save settings
        SaveSystem.SaveSettings(new SettingsData(_tempSoundsVolume, _tempMusicVolume, _tempQualityIndex, _tempIsFullscreen, _tempResolutionIndex));
    }

    private void UpdateSettings()
    {
        // Apply settings changes
        mainMixer.SetFloat("MusicParam", _tempMusicVolume);
        mainMixer.SetFloat("SoundsParam", _tempSoundsVolume);
        QualitySettings.SetQualityLevel(_tempQualityIndex);
        Screen.fullScreen = _tempIsFullscreen;
        Resolution resolution = resolutions[_tempResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        // Set screens to be active/inactive
        optionsScreen.SetActive(false);
        pauseScreen.SetActive(true);
        helpScreen.SetActive(false);
    }

    public void CancelSettings()
    {
        // Apply settings changes
        mainMixer.SetFloat("SoundsParam", _previousSoundsVolume);

        // Set screens visibility
        optionsScreen.SetActive(false);
        pauseScreen.SetActive(true);
        helpScreen.SetActive(false);
    }

    public void Help()
    {
        pauseScreen.SetActive(false);
        optionsScreen.SetActive(false);
        helpScreen.SetActive(true);
    }

    public void BackToPause()
    {
        optionsScreen.SetActive(false);
        pauseScreen.SetActive(true);
        helpScreen.SetActive(false);
    }
}