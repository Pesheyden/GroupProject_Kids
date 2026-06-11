using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class TimedUnityEvent : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _startDealy;
    [SerializeField] private float _endDealy;
    [SerializeField] private int _repsAmount;
    [SerializeField] private bool _startOnAwake;

    [Foldout("Events")] public UnityEvent OnCycleStart;
    [Foldout("Events")] public UnityEvent OnCycleEnd;

    private void Start()
    {
        if(_startOnAwake)
            StartCoroutine(CycleCoroutine());
    }

    public void StartCycle()
    {
        StartCoroutine(CycleCoroutine());
    }

    private IEnumerator CycleCoroutine()
    {
        int reps = _repsAmount;
        while (reps != 0)
        {
            yield return new WaitForSeconds(_startDealy);
            OnCycleStart?.Invoke();

            yield return new WaitForSeconds(_endDealy);
            OnCycleEnd?.Invoke();

            reps--;
        }
    }
}
