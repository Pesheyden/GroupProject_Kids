using UnityEngine;
using UnityEngine.Events;

public class RainbowEndpoint : MonoBehaviour
{
    [SerializeField] private UnityEvent _onHit;
    [SerializeField] private EventSo _onHitEventSo;

    [SerializeField] private bool _fired;
    public void Hit()
    {
        if (_fired)
            return;

        _fired = true;
        _onHit?.Invoke();
        _onHitEventSo?.Raise();


    }
}
