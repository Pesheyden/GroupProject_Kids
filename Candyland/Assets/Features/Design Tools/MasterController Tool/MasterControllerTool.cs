using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class MasterControllerTool : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private List<PlayerMoveControllerRB> _moveControllers = new();
    [SerializeField] private List<PlayerInput> _playerInputs = new();
    [SerializeField] private List<GameObject> _characterCameras = new();

    [Header("Split Screen UI")]
    [SerializeField] private List<GameObject> _playerCameraUI = new();
    [SerializeField] private GameObject _godCameraUI;

    [Header("God Camera")]
    [SerializeField] private GameObject _godCamera;

    private Gamepad _masterGamepad;
    public Gamepad MasterGamepad => _masterGamepad;

    public bool _hasGodMode;
    private int _currentCharacterIndex = 0;

    private void Start()
    {
        if (Gamepad.all.Count > 0)
        {
            _masterGamepad = Gamepad.all[0];
            Debug.Log("Master Controller: " + _masterGamepad.displayName);
        }

        // Cameras stay active because they render to Render Textures.
        for (int i = 0; i < _characterCameras.Count; i++)
        {
            _characterCameras[i].SetActive(true);
        }

        _godCamera.SetActive(false);
        _godCameraUI.SetActive(false);

        ShowPlayerSplitScreen(true);
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
        _godCamera.transform.position = _characterCameras[_currentCharacterIndex].transform.position;
        _godCamera.transform.rotation = _characterCameras[_currentCharacterIndex].transform.rotation;

        _godCamera.SetActive(true);

        ShowPlayerSplitScreen(false);
        _godCameraUI.SetActive(true);
    }

    [Button]
    public void SwitchActionBack()
    {
        _hasGodMode = false;

        _godCamera.SetActive(false);
        _godCameraUI.SetActive(false);

        ShowPlayerSplitScreen(true);
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
        }

        _godCamera.transform.position = _characterCameras[index].transform.position;
        _godCamera.transform.rotation = _characterCameras[index].transform.rotation;

        Debug.Log("Now controlling player: " + index);
    }

    private void ShowPlayerSplitScreen(bool show)
    {
        for (int i = 0; i < _playerCameraUI.Count; i++)
        {
            _playerCameraUI[i].SetActive(show);
        }
    }
}