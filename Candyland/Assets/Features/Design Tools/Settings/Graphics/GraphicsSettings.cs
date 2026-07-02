using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown fpsDropdown;

    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;

    private void Start()
    {
        SetupResolutionDropdown();
        SetupGammaSlider();
        SetupFullscreen();
        SetupVSync();
        SetupQuality();
        SetupFPS();
    }

    void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        Resolution[] resolutions = GraphicsManagement.Instance.GetResolutions();

        for (int i = 0; i < resolutions.Length; i++)
        {
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolutions[i].width + " x " + resolutions[i].height));
        }

        resolutionDropdown.value = GraphicsManagement.Instance.GetResolutionIndex();

        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    void SetupGammaSlider()
    {
        brightnessSlider.minValue = 0.5f;
        brightnessSlider.maxValue = 2f;

        brightnessSlider.value = GraphicsManagement.Instance.GetGamma();

        brightnessSlider.onValueChanged.AddListener(OnGammaChanged);
    }

    public void OnResolutionChanged(int index)
    {
        GraphicsManagement.Instance.SetResolution(index);
    }

    public void OnGammaChanged(float value)
    {
        GraphicsManagement.Instance.SetGamma(value);
    }

    void SetupFullscreen()
    {
        fullscreenToggle.isOn = GraphicsManagement.Instance.GetFullscreen();

        fullscreenToggle.onValueChanged.AddListener(GraphicsManagement.Instance.SetFullscreen);
    }

    void SetupVSync()
    {
        vSyncToggle.isOn = GraphicsManagement.Instance.GetVSync();

        vSyncToggle.onValueChanged.AddListener(GraphicsManagement.Instance.SetVSync);
    }

    void SetupQuality()
    {
        qualityDropdown.ClearOptions();

        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));

        qualityDropdown.value = GraphicsManagement.Instance.GetQuality();

        qualityDropdown.onValueChanged.AddListener(GraphicsManagement.Instance.SetQuality);
    }

    void SetupFPS()
    {
        fpsDropdown.ClearOptions();

        fpsDropdown.AddOptions(
            new System.Collections.Generic.List<string>
            {
                "30",
                "60",
                "120",
                "Unlimited"
            });

        fpsDropdown.value = GraphicsManagement.Instance.GetFPSLimit();

        fpsDropdown.onValueChanged.AddListener(GraphicsManagement.Instance.SetFPSLimit);
    }

    private void OnDestroy()
    {
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        brightnessSlider.onValueChanged.RemoveListener(OnGammaChanged);
    }
}