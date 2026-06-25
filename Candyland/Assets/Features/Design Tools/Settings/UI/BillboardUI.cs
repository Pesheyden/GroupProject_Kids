using System.Collections;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private GameObject _billboardUI;
    [SerializeField] private float _showDuration;
    private Transform _otherPosition;

    void Update()
    {
        _billboardUI.transform.LookAt(_otherPosition);
        _billboardUI.transform.Rotate(0,180,0);
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return;

        PlayerMoveControllerRB player = other.GetComponent<PlayerMoveControllerRB>();
        _otherPosition = player.transform;

        StartCoroutine(ShowBillboard());
    }

    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player")) return;

        _billboardUI.SetActive(false);
    }

    IEnumerator ShowBillboard()
    {
        _billboardUI.SetActive(true);
        yield return new WaitForSeconds(_showDuration);
        _billboardUI.SetActive(false);
    }
}
