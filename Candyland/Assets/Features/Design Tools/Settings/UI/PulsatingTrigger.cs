using UnityEngine;
using UnityEngine.Events;

public class PulsatingTrigger : MonoBehaviour
{
    [Header("Enter Events")]
    [SerializeField] private UnityEvent onMintyEventEnter;
    [SerializeField] private UnityEvent onMarshEventEnter;
    [SerializeField] private UnityEvent onRainbowEventEnter;
    [SerializeField] private UnityEvent onPearlEventEnter;

    [Header("Exit Events")]
    [SerializeField] private UnityEvent onMintyEventExit;
    [SerializeField] private UnityEvent onMarshEventExit;
    [SerializeField] private UnityEvent onRainbowEventExit;
    [SerializeField] private UnityEvent onPearlEventExit;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMoveControllerRB player = other.GetComponent<PlayerMoveControllerRB>();

        if (player == null) return;

        switch(player._currentMonster)
        {
            case PlayerMoveControllerRB.MonsterSelector.Minty:
            onMintyEventEnter.Invoke();
            break;

            case PlayerMoveControllerRB.MonsterSelector.Marsh:
            onMarshEventEnter.Invoke();
            break;

            case PlayerMoveControllerRB.MonsterSelector.Rainbow:
            onRainbowEventEnter.Invoke();
            break;

            case PlayerMoveControllerRB.MonsterSelector.Prul:
            onPearlEventEnter.Invoke();
            break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMoveControllerRB player = other.GetComponent<PlayerMoveControllerRB>();

        if (player == null) return;

        switch(player._currentMonster)
        {
            case PlayerMoveControllerRB.MonsterSelector.Minty:
            onMintyEventExit.Invoke();
            break;

            case PlayerMoveControllerRB.MonsterSelector.Marsh:
            onMarshEventExit.Invoke();
            break;

            case PlayerMoveControllerRB.MonsterSelector.Rainbow:
            onRainbowEventExit.Invoke();
            break;

            case PlayerMoveControllerRB.MonsterSelector.Prul:
            onPearlEventExit.Invoke();
            break;
            
        }
    }
}
