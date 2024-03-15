using System.Collections.Generic;
using Common;
using Save;
using Save.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private MainLevelLoader mainLevelLoader;

    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject optionsScreen;
    [SerializeField] private GameObject helpScreen;

    [Header("Audio")] [SerializeField] private AudioMixer mainMixer;

    private Resolution[] _resolutions;

    [Header("UI Elements")] 
    public Slider soundsVolume;
    public Slider musicVolume;
    public Toggle fullScreenToggle;
    public TMP_Dropdown qualityDropDown;
    public TMP_Dropdown resolutionDropdown;
    public Button continueButton;

    // Temporary variables to store changes
    private float _tempSoundsVolume;
    private float _tempMusicVolume;
    private int _tempQualityIndex;
    private bool _tempIsFullscreen;
    private int _tempResolutionIndex;

    private MainProgressData _mainProgressData;

    // Start is called before the first frame update
    private void Start()
    {
        // Get the resolution dropdown specified for this computer
        _resolutions = Screen.resolutions;
        //resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = _resolutions[i].width + " x " + _resolutions[i].height;
            options.Add(option);
            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        
        // Load saved settings
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

        _mainProgressData = SaveSystem.LoadMainProgress();
        if (_mainProgressData != null)
        {
            // Enable continue button
            continueButton.interactable = true;
        }

    }

    // Main Menu Screen
    public void NewGame()
    {
        //SceneManager.LoadScene("FarmScene");
        mainLevelLoader.InitScene();
    }

    public void Continue()
    {
        if (_mainProgressData == null) return;
        Debug.Log("Scene to load: " + _mainProgressData.sceneIndex);
        mainLevelLoader.LoadScene(_mainProgressData.sceneIndex);
    }

    public void Options()
    {
        // Set the main menu screen to be inactive and the options screen to be active
        mainMenuScreen.SetActive(false);
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
        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (_resolutions[i].width == resolution.width && _resolutions[i].height == resolution.height)
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
    public void SetSoundsVolume(float volume)
    {
        _tempSoundsVolume = volume;
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
        mainMixer.SetFloat("SoundsParam", _tempSoundsVolume);
        mainMixer.SetFloat("MusicParam", _tempMusicVolume);
        QualitySettings.SetQualityLevel(_tempQualityIndex);
        Screen.fullScreen = _tempIsFullscreen;
        Resolution resolution = _resolutions[_tempResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        
        // Set screens to be active/inactive
        mainMenuScreen.SetActive(true);
        optionsScreen.SetActive(false);
        helpScreen.SetActive(false);
    }

    public void CancelSettings()
    {
        // Set screens to be active/inactive
        mainMenuScreen.SetActive(true);
        optionsScreen.SetActive(false);
        helpScreen.SetActive(false);
    }

    public void Help()
    {
        mainMenuScreen.SetActive(false);
        optionsScreen.SetActive(false);
        helpScreen.SetActive(true);
    }

    public void BackToMainMenu()
    {
        optionsScreen.SetActive(false);
        mainMenuScreen.SetActive(true);
        helpScreen.SetActive(false);
    }
}