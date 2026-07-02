using UnityEngine;
using UnityEngine.InputSystem;

public class TwoPlayerStartButton : MonoBehaviour
{
    public bool AllConnected { get; private set; }

    private void Update()
    {
        AllConnected = Gamepad.all.Count >= 2;
    }
}