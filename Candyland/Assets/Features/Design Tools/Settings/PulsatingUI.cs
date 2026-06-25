using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PulsatingUI : MonoBehaviour
{
    [SerializeField] private float _pulseSpeed;
    [SerializeField] private Image uiImage;
    [SerializeField] private Sprite _unlitImage;
    [SerializeField] private Sprite _litImage;
    [SerializeField] private Animation _litAnimation;
    private bool isPulsating;

    void Awake()
    {
        isPulsating = false;
    }

    public void StartPulsating()
    {
        if(isPulsating) return;

        isPulsating = true;
        ImageChange();
    }

    public void StopPulsating()
    {
        if(!isPulsating) return;

        isPulsating = false;
        uiImage.sprite = _unlitImage;
    }

    void ImageChange()
    {         
        uiImage.sprite = _litImage;
        _litAnimation.Play();
    }
}
