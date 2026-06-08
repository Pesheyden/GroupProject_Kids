using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    public void Started(PlayerInput playerInput);
    public void Canceled(PlayerInput playerInput);
}
