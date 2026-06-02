using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class FlowerGrower : MonoBehaviour
{
    [SerializeField] private List<FlowerPower> flowers = new List<FlowerPower>();
    void Start()
    {
        StartCoroutine(GrowBigFlower());
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            StartCoroutine(ShrinkBigFlower());
        }
    }

    IEnumerator GrowBigFlower()
    {
        for (int i = 0; i < flowers.Count; i++)
        {
            yield return new WaitForSeconds(2f);
            StartCoroutine(flowers[i].GrowFlower());
        }
    }

    IEnumerator ShrinkBigFlower()
    {
        for (int i = 0; i <flowers.Count; i++)
        {
            yield return new WaitForSeconds(2f);
            StartCoroutine(flowers[i].ShrinkFlower());       
        }
    }
}
