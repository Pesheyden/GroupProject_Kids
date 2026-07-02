using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PulsatingUI : MonoBehaviour
{
    [SerializeField] private float _pulseSpeed;
    [SerializeField] private Image uiImage;
    [SerializeField] private Sprite _unlitImage;
    [SerializeField] private Sprite _litImage;
    private bool isPulsating;

    void Awake()
    {
        isPulsating = false;
    }

    public void StartPulsating()
    {
        if(isPulsating) return;

        isPulsating = true;
        StartCoroutine(Pulsate());
    }

    private IEnumerator Pulsate()
    {
        while (isPulsating)
        {
            uiImage.sprite = _litImage;
            yield return new WaitForSeconds(_pulseSpeed);
            uiImage.sprite = _unlitImage;
            yield return new WaitForSeconds(_pulseSpeed);
        }
    }
    public void StopPulsating()
    {
        if(!isPulsating) return;

        isPulsating = false;
        uiImage.sprite = _unlitImage;
    }
}
