using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OpenSettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private List<Button> buttons = new List<Button>();
    [SerializeField] private GameObject firstSettingsSelected;
    [SerializeField] private GameObject firstMainMenuSelected;

    public void SettingsMenuOpen()
    {
        settingsPanel.SetActive(true);

        for(int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSettingsSelected);
    }

    public void SettingsMenuClose()
    {
        settingsPanel.SetActive(false);
        for(int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = true;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstMainMenuSelected);
    }
}
