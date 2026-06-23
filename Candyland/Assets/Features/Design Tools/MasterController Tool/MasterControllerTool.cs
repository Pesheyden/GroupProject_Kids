using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using UnityEngine.InputSystem.Utilities;

public class MasterControllerTool : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private List<PlayerMoveControllerRB> _moveControllers = new();
    [SerializeField] private List<PlayerInput> _playerInputs = new();

    [Header("Split Screen UI")]
    [SerializeField] private GameObject _godCameraUI;

    [Header("God Camera")]
    [SerializeField] private GameObject _godCamera;

    private Gamepad _masterGamepad;
    public Gamepad MasterGamepad => _masterGamepad;

    private List<InputDevice[]>_basicControlDevices = new();

    public bool _hasGodMode;
    private int _currentCharacterIndex = 0;

    private void Start()
    {
        if (Gamepad.all.Count > 0)
        {
            _masterGamepad = Gamepad.all[0];
            Debug.Log("Master Controller: " + _masterGamepad.displayName);
        }
        

        foreach (var playerInput in _playerInputs)
        {
            _basicControlDevices.Add(playerInput.devices.ToArray());
        }

        _godCamera.SetActive(false);
        _godCameraUI.SetActive(false);
        
        SetActiveCharacter(0);
    }

    private void Update()
    {
        if (_masterGamepad == null)
            return;

        if (_masterGamepad.rightShoulder.wasPressedThisFrame)
        {
            if (_hasGodMode)
                SwitchActionBack();
            else
                SwitchAction();
        }

        if (!_hasGodMode && _masterGamepad.leftShoulder.wasPressedThisFrame)
        {
            SwitchCharacter();
        }

        if (!_hasGodMode && _masterGamepad.leftTrigger.wasPressedThisFrame)
        {
            
        }
    }

    [Button]
    public void SwitchAction()
    {
        _hasGodMode = true;

        // Nobody should move while god mode is active.
        for (int i = 0; i < _moveControllers.Count; i++)
        {
            _moveControllers[i].enabled = false;
        }

        for (int i = 0; i < _playerInputs.Count; i++)
        {
            _playerInputs[i].enabled = false;
        }

        // Put god camera at current selected player's camera.
        _godCamera.transform.position = _moveControllers[0].transform.position;
        _godCamera.transform.rotation = _moveControllers[0].transform.rotation;

        _godCamera.SetActive(true);
        
        _godCameraUI.SetActive(true);
    }

    [Button]
    public void SwitchActionBack()
    {
        _hasGodMode = false;

        _godCamera.SetActive(false);
        _godCameraUI.SetActive(false);
        
        SetActiveCharacter(_currentCharacterIndex);
    }

    [Button]
    public void SwitchCharacter()
    {
        _currentCharacterIndex++;

        if (_currentCharacterIndex >= _moveControllers.Count)
        {
            _currentCharacterIndex = 0;
        }

        SetActiveCharacter(_currentCharacterIndex);
    }

    private void SetActiveCharacter(int index)
    {
        for (int i = 0; i < _moveControllers.Count; i++)
        {
            bool isSelected = i == index;

            _moveControllers[i].enabled = isSelected;
            _playerInputs[i].enabled = isSelected;

            if (isSelected && _masterGamepad != null)
            {
                _playerInputs[i].SwitchCurrentControlScheme("Gamepad", _masterGamepad);
            }
            else
            {
                _playerInputs[i].SwitchCurrentControlScheme("Gamepad", _basicControlDevices[i]);
            }
        }
        

        Debug.Log("Now controlling player: " + index);
    }
    
}