using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MenuSettings : MonoBehaviour
{
    private float musicVolume = 0.2f;

    [SerializeField]
    private AudioMixer _mixer;
    [SerializeField]
    private Slider _master;
    [SerializeField]
    private Slider _music;
    [SerializeField]
    private Slider _effects;

    public Dropdown resolutionDropDown;

    Resolution[] resolutions;

    // Use this for initialization
    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropDown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolution = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;

            options.Add(option);

            if ( i > 0 && resolutions[i].width == resolutions[i - 1].width && resolutions[i].height == resolutions[i - 1].height)
            {
                options.Remove(option);
            }

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolution = i;
            }
        }

        resolutionDropDown.AddOptions(options);
        resolutionDropDown.value = currentResolution;
        resolutionDropDown.RefreshShownValue();

        InitAudio();
    }

    private void InitAudio()
    {
        float masterVolume = PlayerPrefs.GetFloat("Master", 1);
        float musicVolume = PlayerPrefs.GetFloat("Music", 1);
        float effectsVolume = PlayerPrefs.GetFloat("Effects", 1);

        SetMasterLevel(masterVolume);
        SetMusicLevel(musicVolume);
        SetEffectsLevel(effectsVolume);

        _master.value = masterVolume;
        _music.value = musicVolume;
        _effects.value = effectsVolume;
    }

    public void SetMasterLevel(float sliderValue)
    {
        SetAudioLevel("Master", sliderValue);
    }

    public void SetMusicLevel(float sliderValue)
    {
        SetAudioLevel("Music", sliderValue);
    }

    public void SetEffectsLevel(float sliderValue)
    {
        SetAudioLevel("Effects", sliderValue);
    }

    private void SetAudioLevel(string audioType, float value)
    {
        PlayerPrefs.SetFloat(audioType, float.Parse(value.ToString("n2")));
        _mixer.SetFloat(audioType, Mathf.Log10(value) * 20); // 
    }

    public void SetQuality (int qualitySetting)
    {
        QualitySettings.SetQualityLevel(qualitySetting, true);
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int currentResolution)
    {
        Resolution resolution = resolutions[currentResolution];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}