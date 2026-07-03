using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class OpenSettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private List<Button> buttons = new List<Button>();
    [SerializeField] private GameObject firstSettingsSelected;
    [SerializeField] private GameObject firstMainMenuSelected;
    bool menuIsOpened;

    void Update()
    {
        /*if(menuIsOpened && Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            SettingsMenuClose();
        }*/

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (menuIsOpened)
                EventSystem.current.SetSelectedGameObject(firstSettingsSelected);
            else
                EventSystem.current.SetSelectedGameObject(firstMainMenuSelected);
        }
    }

    public void SettingsMenuOpen()
    {
        settingsPanel.SetActive(true);
        menuIsOpened = true;

        for(int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;
        }

        StartCoroutine(SelectAfterFrame(firstSettingsSelected));
    }

    public void SettingsMenuClose()
    {
        settingsPanel.SetActive(false);
        menuIsOpened = false;

        for(int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = true;
        }

        StartCoroutine(SelectAfterFrame(firstMainMenuSelected));
    }

    private IEnumerator SelectAfterFrame(GameObject selectedObject)
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectedObject);
    }
}
