using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TwoPlayerSwitcher : MonoBehaviour
{
    [Header("Character Prefabs")]
    [SerializeField] private List<GameObject> characterPrefabsFirst = new List<GameObject>();
    [SerializeField] private List<GameObject> characterPrefabsSecond = new List<GameObject>();

    [Header("Current Spawned Characters")]
    [SerializeField] private GameObject playerOneCharacter;
    [SerializeField] private GameObject playerTwoCharacter;

    [SerializeField] private Transform playerOneSlot;
    [SerializeField] private Transform playerTwoSlot;

    [Header("Radial Menus")]
    [SerializeField] private GameObject _radialMenu1;
    [SerializeField] private GameObject _radialMenu2;

    [SerializeField] private GameObject _firstButtonPlayer1;
    [SerializeField] private GameObject _firstButtonPlayer2;

    [Header("Players index")]
    [Tooltip("Type the index of the character you choose before starting: Minty - 0; Marsh - 1; Rainbow - 2; Pearl - 3")]
    [SerializeField] private int playerOneIndex = 0;
    [SerializeField] private int playerTwoIndex = 1;

    private PlayerMoveControllerRB moveControllerFirst;
    private PlayerMoveControllerRB moveControllerSecond;

    private Gamepad playerOneGamepad;
    private Gamepad playerTwoGamepad;

    private void Start()
    {
        if (Gamepad.all.Count > 0)
            playerOneGamepad = Gamepad.all[0];

        if (Gamepad.all.Count > 1)
            playerTwoGamepad = Gamepad.all[1];
    }

    private void Update()
    {
        if (playerOneCharacter != null)
        {
            moveControllerFirst = playerOneCharacter.GetComponentInChildren<PlayerMoveControllerRB>();

            playerOneSlot.position = moveControllerFirst.transform.position;
            playerOneSlot.rotation = moveControllerFirst.transform.rotation;
        }

        if (playerTwoCharacter != null)
        {
            moveControllerSecond = playerTwoCharacter.GetComponentInChildren<PlayerMoveControllerRB>();

            playerTwoSlot.position = moveControllerSecond.transform.position;
            playerTwoSlot.rotation = moveControllerSecond.transform.rotation;
        }

        if (playerOneGamepad != null)
        {
            moveControllerFirst = playerOneCharacter.GetComponentInChildren<PlayerMoveControllerRB>();

            if(playerOneGamepad.buttonWest.wasPressedThisFrame)
            {
                OpenRadialMenuFirst();
                moveControllerFirst.enabled = false;
            }
            else if(playerOneGamepad.buttonEast.wasPressedThisFrame && _radialMenu1.activeSelf)
            {
                CloseRadialMenuFirst();
                moveControllerFirst.enabled = true;
            }
        }

        if (playerTwoGamepad != null)
        {
            moveControllerSecond = playerTwoCharacter.GetComponentInChildren<PlayerMoveControllerRB>();

            if(playerTwoGamepad.buttonWest.wasPressedThisFrame)
            {
                OpenRadialMenuSecond();
                moveControllerSecond.enabled = false;
            }
            else if(playerTwoGamepad.buttonEast.wasPressedThisFrame && _radialMenu2.activeSelf)
            {
                CloseRadialMenuSecond();
                moveControllerSecond.enabled = true;

            }
        }
    }

    public void OpenRadialMenuFirst()
    {
        _radialMenu1.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_firstButtonPlayer1);
    }

    public void OpenRadialMenuSecond()
    {
        _radialMenu2.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_firstButtonPlayer2);

    }

    void CloseRadialMenuFirst()
    {
        _radialMenu1.SetActive(false);

        if (moveControllerFirst != null)
            moveControllerFirst.enabled = true;

        EventSystem.current.SetSelectedGameObject(null);
    }

    void CloseRadialMenuSecond()
    {
        _radialMenu2.SetActive(false);

        if (moveControllerSecond != null)
            moveControllerSecond.enabled = true;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SwitchPlayerOneMinty()
    {
        Debug.Log(1);
        if(playerTwoIndex == 0)
        {
            CloseRadialMenuFirst();
            moveControllerFirst.enabled = true;
            return;
        }

        Vector3 oldPosition = playerOneSlot.position;
        Quaternion oldRotation = playerOneSlot.rotation;

        CloseRadialMenuFirst();
        Destroy(playerOneCharacter);
                Debug.Log(1);
        SpawnPlayerOneMinty(playerOneIndex, oldPosition, oldRotation);
    }

    public void SwitchPlayerTwoMinty()
    {
                Debug.Log(11);
        if(playerOneIndex == 0)
        {
            CloseRadialMenuSecond();
            moveControllerSecond.enabled = true;
            return;
        }

        Vector3 oldPosition = playerTwoSlot.position;
        Quaternion oldRotation = playerTwoSlot.rotation;

        CloseRadialMenuSecond();
        Destroy(playerTwoCharacter);
        SpawnPlayerTwoMinty(playerTwoIndex, oldPosition, oldRotation);
    }

    public void SwitchPlayerOneMarsh()
    {
                Debug.Log(2);
        if(playerTwoIndex == 1)
        {
            CloseRadialMenuFirst();
            moveControllerFirst.enabled = true;
            return;
        }

        Vector3 oldPosition = playerOneSlot.position;
        Quaternion oldRotation = playerOneSlot.rotation;

        CloseRadialMenuFirst();
        Destroy(playerOneCharacter);
        SpawnPlayerOneMarsh(playerOneIndex, oldPosition, oldRotation);
    }

    public void SwitchPlayerTwoMarsh()
    {
                Debug.Log(22);
        if(playerOneIndex == 1)
        {
            CloseRadialMenuSecond();
            moveControllerSecond.enabled = true;
            return;
        }

        Vector3 oldPosition = playerTwoSlot.position;
        Quaternion oldRotation = playerTwoSlot.rotation;

        CloseRadialMenuSecond();
        Destroy(playerTwoCharacter);
        SpawnPlayerTwoMarsh(playerTwoIndex, oldPosition, oldRotation);
    }

    public void SwitchPlayerOneRainbow()
    {
                Debug.Log(3);
        if(playerTwoIndex == 2)
        {
            CloseRadialMenuFirst();
            moveControllerFirst.enabled = true;
            return;
        }

        Vector3 oldPosition = playerOneSlot.position;
        Quaternion oldRotation = playerOneSlot.rotation;

        CloseRadialMenuFirst();
        Destroy(playerOneCharacter);
        SpawnPlayerOneRainbow(playerOneIndex, oldPosition, oldRotation);
    }

    public void SwitchPlayerTwoRainbow()
    {
        Debug.Log(33);
        if(playerOneIndex == 2)
        {
            CloseRadialMenuSecond();
            moveControllerSecond.enabled = true;
            return;
        }

        Vector3 oldPosition = playerTwoSlot.position;
        Quaternion oldRotation = playerTwoSlot.rotation;

        CloseRadialMenuSecond();
        Destroy(playerTwoCharacter);
        SpawnPlayerTwoRainbow(playerTwoIndex, oldPosition, oldRotation);
    }

    public void SwitchPlayerOnePearl()
    {
        if(playerTwoIndex == 3)
        {
            CloseRadialMenuFirst();
            moveControllerFirst.enabled = true;
            return;
        }

        Vector3 oldPosition = playerOneSlot.position;
        Quaternion oldRotation = playerOneSlot.rotation;

        CloseRadialMenuFirst();
        Destroy(playerOneCharacter);
        SpawnPlayerOnePearl(playerOneIndex, oldPosition, oldRotation);
    }

    public void SwitchPlayerTwoPearl()
    {
        if(playerOneIndex == 3)
        {
            CloseRadialMenuSecond();
            moveControllerSecond.enabled = true;
            return;
        }

        Vector3 oldPosition = playerTwoSlot.position;
        Quaternion oldRotation = playerTwoSlot.rotation;

        CloseRadialMenuSecond();
        Destroy(playerTwoCharacter);
        SpawnPlayerTwoPearl(playerTwoIndex, oldPosition, oldRotation);
    }

    private void SpawnPlayerOneMinty(int index, Vector3 position, Quaternion rotation)
    {
        playerOneCharacter = Instantiate(characterPrefabsFirst[0], position, rotation);

        moveControllerFirst = playerOneCharacter.GetComponentInChildren<PlayerMoveControllerRB>();
        moveControllerFirst.transform.position = position;
        moveControllerFirst.transform.rotation = rotation;
        moveControllerFirst.enabled = true;
        index = 0;
        playerOneIndex = index;
        Debug.Log("1 " + position);
    }

    private void SpawnPlayerTwoMinty(int index, Vector3 position, Quaternion rotation)
    {
        playerTwoCharacter = Instantiate(characterPrefabsSecond[0], position, rotation);

        moveControllerSecond = playerTwoCharacter.GetComponentInChildren<PlayerMoveControllerRB>();
        moveControllerSecond.transform.position = position;
        moveControllerSecond.transform.rotation = rotation;
        moveControllerSecond.enabled = true;
        index = 0;
        playerTwoIndex = index;
                Debug.Log(position);
    }

    private void SpawnPlayerOneMarsh(int index, Vector3 position, Quaternion rotation)
    {
        playerOneCharacter = Instantiate(characterPrefabsFirst[1], position, rotation);

        moveControllerFirst = playerOneCharacter.GetComponentInChildren<PlayerMoveControllerRB>();
        moveControllerFirst.transform.position = position;
        moveControllerFirst.transform.rotation = rotation;
        moveControllerFirst.enabled = true;
        index = 1;
        playerOneIndex = index;
    }

    private void SpawnPlayerTwoMarsh(int index, Vector3 position, Quaternion rotation)
    {
        playerTwoCharacter = Instantiate(characterPrefabsSecond[1], position, rotation);

        moveControllerSecond = playerTwoCharacter.GetComponentInChildren<PlayerMoveControllerRB>();
        moveControllerSecond.transform.position = position;
        moveControllerSecond.transform.rotation = rotation;
        moveControllerSecond.enabled = true;
        index = 1;
        playerTwoIndex = index;
    }
    private void SpawnPlayerOneRainbow(int index, Vector3 position, Quaternion rotation)
    {
        playerOneCharacter = Instantiate(characterPrefabsFirst[2], position, rotation);

        moveControllerFirst = playerOneCharacter.GetComponentInChildren<PlayerMoveControllerRB>();
        moveControllerFirst.transform.position = position;
        moveControllerFirst.transform.rotation = rotation;
        moveControllerFirst.enabled = true;
        index = 2;
        playerOneIndex = index;
    }

    private void SpawnPlayerTwoRainbow(int index, Vector3 position, Quaternion rotation)
    {
        playerTwoCharacter = Instantiate(characterPrefabsSecond[2], position, rotation);

        moveControllerSecond = playerTwoCharacter.GetComponentInChildren<PlayerMoveControllerRB>();
        moveControllerSecond.transform.position = position;
        moveControllerSecond.transform.rotation = rotation;
        moveControllerSecond.enabled = true;
        index = 2;
        playerTwoIndex = index;
    }

    private void SpawnPlayerOnePearl(int index, Vector3 position, Quaternion rotation)
    {
        playerOneCharacter = Instantiate(characterPrefabsFirst[3], position, rotation);

        moveControllerFirst = playerOneCharacter.GetComponentInChildren<PlayerMoveControllerRB>();
        moveControllerFirst.transform.position = position;
        moveControllerFirst.transform.rotation = rotation;
        moveControllerFirst.enabled = true;
        index = 3;
        playerOneIndex = index;
    }

    private void SpawnPlayerTwoPearl(int index, Vector3 position, Quaternion rotation)
    {
        playerTwoCharacter = Instantiate(characterPrefabsSecond[3], position, rotation);
        
        moveControllerSecond = playerTwoCharacter.GetComponentInChildren<PlayerMoveControllerRB>();
        moveControllerSecond.transform.position = position;
        moveControllerSecond.transform.rotation = rotation;
        moveControllerSecond.enabled = true;
        index = 3;
        playerTwoIndex = index;
    }

}
