using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GraphicsManagement : MonoBehaviour
{
    public static GraphicsManagement Instance;

    [Header("Graphics")]
    [Range(0.5f, 2f)]
    [SerializeField] private float brightness = 1f;

    [SerializeField] private int resolutionIndex;
    [SerializeField] private Volume volume;

    [Header("Display")]
    [SerializeField] private bool fullScreen = true;
    [SerializeField] private int qualityIndex;
    [SerializeField] private int fpsLimitIndex;

    private readonly int[] fpsOptions = { 30, 60, 120, -1 };
    private ColorAdjustments colorAdjustments;

    private Resolution[] resolutions;

    void Start()
    {
        volume.profile.TryGet(out colorAdjustments);

        colorAdjustments.postExposure.value = brightness;

        SetResolution(resolutionIndex);
        SetFullscreen(fullScreen);
        SetQuality(qualityIndex);
        SetFPSLimit(fpsLimitIndex);

        SetResolution(resolutionIndex);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            resolutions = Screen.resolutions;

            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Resolution[] GetResolutions()
    {
        return resolutions;
    }

    public int GetResolutionIndex()
    {
        return resolutionIndex;
    }

    public float GetGamma()
    {
        return brightness;
    }

    public void SetResolution(int index)
    {
        resolutionIndex = index;

        Resolution res = resolutions[index];

        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

        PlayerPrefs.SetInt("Resolution", resolutionIndex);
        PlayerPrefs.Save();

        Debug.Log($"Resolution: {res.width}x{res.height}");
    }

    public void SetGamma(float value)
    {
        brightness = value;

        //Apply gamma to a global post-processing volume or shader exposure here
        colorAdjustments.postExposure.value = brightness;

        PlayerPrefs.SetFloat("Gamma", brightness);
        PlayerPrefs.Save();

        Debug.Log($"Gamma: {brightness:F2}");
    }

    public void SetFullscreen(bool value)
    {
        fullScreen = value;

        Screen.fullScreen = fullScreen;

        PlayerPrefs.SetInt("Fullscreen", fullScreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Fullscreen: " + fullScreen);
    }

    public bool GetFullscreen()
    {
        return fullScreen;
    }

    public void SetQuality(int index)
    {
        qualityIndex = index;

        QualitySettings.SetQualityLevel(index);

        PlayerPrefs.SetInt("Quality", qualityIndex);
        PlayerPrefs.Save();

        //Debug.Log("Quality: " + QualitySettings.names[index]);
    }

    public int GetQuality()
    {
        return qualityIndex;
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("VSync: " + enabled);
    }

    public bool GetVSync()
    {
        return QualitySettings.vSyncCount > 0;
    }

    public void SetFPSLimit(int index)
    {
        fpsLimitIndex = index;

        Application.targetFrameRate = fpsOptions[index];

        PlayerPrefs.SetInt("FPSLimit", fpsLimitIndex);
        PlayerPrefs.Save();

        Debug.Log("FPS Limit: " + fpsOptions[index]);
    }

    public int GetFPSLimit()
    {
        return fpsLimitIndex;
    }

    private void LoadSettings()
    {
        brightness = PlayerPrefs.GetFloat("Gamma", 0f);

        resolutionIndex = PlayerPrefs.GetInt("Resolution", resolutions.Length - 1);

        fullScreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        qualityIndex = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());

        fpsLimitIndex = PlayerPrefs.GetInt("FPSLimit", 1);
    }
}