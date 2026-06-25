using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioManagement : MonoBehaviour
{
    public static AudioManagement Instance;

    [Header("FMOD VCAs")]
    private VCA masterVCA;
    private VCA musicVCA;
    private VCA ambienceVCA;
    private VCA sfxVCA;

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float ambienceVolume = 1f;
    private float sfxVolume = 1f;

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetAmbienceVolume() => ambienceVolume;
    public float GetSFXVolume() => sfxVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            masterVCA = RuntimeManager.GetVCA("vca:/Master");
            musicVCA = RuntimeManager.GetVCA("vca:/Music");
            ambienceVCA = RuntimeManager.GetVCA("vca:/Ambience");
            sfxVCA = RuntimeManager.GetVCA("vca:/SFX");

            LoadSettings();
            ApplyVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        masterVCA.setVolume(masterVolume);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        musicVCA.setVolume(musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    public void SetAmbienceVolume(float value)
    {
        ambienceVolume = Mathf.Clamp01(value);
        ambienceVCA.setVolume(ambienceVolume);
        PlayerPrefs.SetFloat("AmbienceVolume", ambienceVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        sfxVCA.setVolume(sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        ambienceVolume = PlayerPrefs.GetFloat("AmbienceVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    private void ApplyVolumes()
    {
        masterVCA.setVolume(masterVolume);
        musicVCA.setVolume(musicVolume);
        ambienceVCA.setVolume(ambienceVolume);
        sfxVCA.setVolume(sfxVolume);
    }
}