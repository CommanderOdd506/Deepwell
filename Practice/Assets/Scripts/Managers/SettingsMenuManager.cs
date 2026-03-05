using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI; 

public class SettingsMenuManager : MonoBehaviour
{
    public AudioMixer mainAudioMixer;

    public MouseLook mouseLook;
    public Toggle fullscreenToggle;
    public string masterChannel = "MasterVol";

    public Slider vol;
    public Slider mouseSens;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeMouseSensetivity();
        ChangeVolume();
    }
    public void OnEnable()
    {
        LoadUISettings();
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeVolume()
    {
        if (vol.value < 0.02)
        {
            mainAudioMixer.SetFloat(masterChannel, -80);
        }
        else
        {
            mainAudioMixer.SetFloat(masterChannel, Mathf.Log10(vol.value) * 20);
        }

    }

    public void LoadUISettings()
    {
        float masterDb;

        if (mainAudioMixer.GetFloat(masterChannel, out masterDb))
            vol.value = Mathf.Pow(10f, masterDb / 20f);

        float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", .3f);
        mouseSens.value = savedSens;

        int savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1);
        bool isFullscreen = savedFullscreen == 1;
    }
    public void ChangeMouseSensetivity()
    {

        float sens = mouseSens.value;

        mouseLook.SetSensitivity(sens);

        PlayerPrefs.SetFloat("MouseSensitivity", sens);
        PlayerPrefs.Save();
    }
    public void ChangeFullscreen()
    {
        bool isFullscreen = fullscreenToggle.isOn;
        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
