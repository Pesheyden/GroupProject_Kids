using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _ambienceSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        SetupSliders();
    }

    private void SetupSliders()
    {
        if (AudioManagement.Instance == null)
        {
            Debug.LogError("AudioManagement not found in scene.");
            return;
        }

        SetupSlider(_masterSlider, AudioManagement.Instance.GetMasterVolume(), OnMasterChanged);
        SetupSlider(_musicSlider, AudioManagement.Instance.GetMusicVolume(), OnMusicChanged);
        SetupSlider(_ambienceSlider, AudioManagement.Instance.GetAmbienceVolume(), OnAmbienceChanged);
        SetupSlider(_sfxSlider, AudioManagement.Instance.GetSFXVolume(), OnSFXChanged);
    }

    private void SetupSlider(Slider slider, float startValue, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = startValue;

        slider.onValueChanged.AddListener(callback);
    }

    private void OnMasterChanged(float value)
    {
        AudioManagement.Instance.SetMasterVolume(value);
    }

    private void OnMusicChanged(float value)
    {
        AudioManagement.Instance.SetMusicVolume(value);
    }

    private void OnAmbienceChanged(float value)
    {
        AudioManagement.Instance.SetAmbienceVolume(value);
    }

    private void OnSFXChanged(float value)
    {
        AudioManagement.Instance.SetSFXVolume(value);
    }

    private void OnDestroy()
    {
        if (_masterSlider != null)
            _masterSlider.onValueChanged.RemoveListener(OnMasterChanged);

        if (_musicSlider != null)
            _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);

        if (_ambienceSlider != null)
            _ambienceSlider.onValueChanged.RemoveListener(OnAmbienceChanged);

        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
    }
}