using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TwoPlayerSwitcher : MonoBehaviour
{
    [Header("Character Prefabs")]
    [SerializeField] private List<GameObject> characterPrefabs = new List<GameObject>();

    /*[Header("Starting Spawn Points")]
    [SerializeField] private Transform playerOneStart;
    [SerializeField] private Transform playerTwoStart;*/

    [Header("Current Spawned Characters")]
    [SerializeField] private GameObject playerOneCharacter;
    [SerializeField] private GameObject playerTwoCharacter;

    private int playerOneIndex = 0;
    private int playerTwoIndex = 1;

    private Gamepad playerOneGamepad;
    private Gamepad playerTwoGamepad;

    private void Start()
    {
        if (Gamepad.all.Count > 0)
            playerOneGamepad = Gamepad.all[0];

        if (Gamepad.all.Count > 1)
            playerTwoGamepad = Gamepad.all[1];

        /*SpawnPlayerOne(playerOneIndex, playerOneStart.position, playerOneStart.rotation);
        SpawnPlayerTwo(playerTwoIndex, playerTwoStart.position, playerTwoStart.rotation);*/
    }

    private void Update()
    {
        if (playerOneGamepad != null)
        {
            if (playerOneGamepad.dpad.right.wasPressedThisFrame)
                SwitchPlayerOne(1);

            if (playerOneGamepad.dpad.left.wasPressedThisFrame)
                SwitchPlayerOne(-1);
        }

        if (playerTwoGamepad != null)
        {
            if (playerTwoGamepad.dpad.right.wasPressedThisFrame)
                SwitchPlayerTwo(1);

            if (playerTwoGamepad.dpad.left.wasPressedThisFrame)
                SwitchPlayerTwo(-1);
        }
    }

    private void SwitchPlayerOne(int direction)
    {
        Vector3 oldPosition = playerOneCharacter.transform.position;
        Quaternion oldRotation = playerOneCharacter.transform.rotation;

        playerOneIndex = GetNextAvailableIndex(playerOneIndex, playerTwoIndex, direction);

        Destroy(playerOneCharacter);
        SpawnPlayerOne(playerOneIndex, oldPosition, oldRotation);
    }

    private void SwitchPlayerTwo(int direction)
    {
        Vector3 oldPosition = playerTwoCharacter.transform.position;
        Quaternion oldRotation = playerTwoCharacter.transform.rotation;

        playerTwoIndex = GetNextAvailableIndex(playerTwoIndex, playerOneIndex, direction);

        Destroy(playerTwoCharacter);
        SpawnPlayerTwo(playerTwoIndex, oldPosition, oldRotation);
    }

    private int GetNextAvailableIndex(int currentIndex, int blockedIndex, int direction)
    {
        int nextIndex = currentIndex;

        do
        {
            nextIndex += direction;

            if (nextIndex >= characterPrefabs.Count)
                nextIndex = 0;

            if (nextIndex < 0)
                nextIndex = characterPrefabs.Count - 1;

        } while (nextIndex == blockedIndex);

        return nextIndex;
    }

    private void SpawnPlayerOne(int index, Vector3 position, Quaternion rotation)
    {
        playerOneCharacter = Instantiate(characterPrefabs[index], position, rotation);
    }

    private void SpawnPlayerTwo(int index, Vector3 position, Quaternion rotation)
    {
        playerTwoCharacter = Instantiate(characterPrefabs[index], position, rotation);
    }
}
