using UnityEngine;
using System.Collections;
using BSOAP.Variables;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Net.NetworkInformation;
using System;
using NaughtyAttributes;
using UnityUtils;


[Serializable]
public class FlowerSection
{
    public Transform flowerStem;
    public List<Transform> flowerParts = new List<Transform>();
}

public class FlowerPower : MonoBehaviour, IInteractable
{
    [ReadOnly] [SerializeField] private List<FlowerSection> flowerSections;
    [SerializeField] private float _stemGrowTime;
    [SerializeField] private float _leaveGrowTime;
    [SerializeField] private float _petalsGrowTime;

    private bool isGrowing = false;
    private bool isGrown = false;

    void Start()
    {
        flowerSections = new List<FlowerSection>();
        foreach (var stem in transform.Children())
        {
            var section = new FlowerSection();
            flowerSections.Add(section);
            section.flowerStem = stem;
            foreach (var part in stem.Children())
            {
                section.flowerParts.Add(part);
                part.localScale = Vector3.zero;
            }

            stem.localScale = new Vector3(1, 0, 1);
            stem.gameObject.SetActive(false);
        }
    }

    IEnumerator GrowPart(Transform part, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float time = timer / duration;

            part.localScale = Vector3.Lerp(startScale, endScale, time);

            yield return null;
        }

        part.localScale = endScale;
    }

    IEnumerator GrowStem(Transform stem, float duration)
    {
        Vector3 startScale = new Vector3(1, 0, 1);
        Vector3 endScale = Vector3.one;

        float timer = 0f;

        stem.gameObject.SetActive(true);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float time = timer / duration;

            stem.localScale = Vector3.Lerp(startScale, endScale, time);

            yield return null;
        }

        stem.localScale = endScale;
    }

    IEnumerator ShrinkPart(Transform part, float duration)
    {
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float time = timer / duration;

            part.localScale = Vector3.Lerp(startScale, endScale, time);

            yield return null;
        }

        part.localScale = endScale;
    }

    IEnumerator ShrinkStem(Transform stem, float duration)
    {
        Vector3 startScale = Vector3.one;
        Vector3 endScale = new Vector3(1, 0, 1);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float time = timer / duration;

            stem.localScale = Vector3.Lerp(startScale, endScale, time);


            yield return null;
        }

        stem.localScale = endScale;
        stem.gameObject.SetActive(false);
    }

    public IEnumerator GrowFlower()
    {
        isGrowing = true;
        isGrown = true;


        foreach (var section in flowerSections)
        {
            var flowerStems = section.flowerStem;
            var flowerLeaves = section.flowerParts;

            if (flowerStems != null)
            {
                StartCoroutine(GrowStem(flowerStems, _stemGrowTime));
                yield return new WaitForSeconds(_stemGrowTime);
            }

            if (flowerLeaves != null)
            {
                for (int i = 0; i < flowerLeaves.Count; i++)
                {
                    StartCoroutine(GrowPart(flowerLeaves[i], _leaveGrowTime));
                    yield return new WaitForSeconds(_leaveGrowTime);
                }
            }
        }

        isGrowing = false;
    }

    public IEnumerator ShrinkFlower()
    {
        isGrowing = true;

        for (int j = flowerSections.Count - 1; j >= 0; j--)
        {
            FlowerSection section = flowerSections[j];
            var flowerStems = section.flowerStem;
            var flowerLeaves = section.flowerParts;


            if (flowerLeaves != null)
            {
                for (int i = flowerLeaves.Count - 1; i >= 0; i--)
                {
                    StartCoroutine(ShrinkPart(flowerLeaves[i], _leaveGrowTime));
                    yield return new WaitForSeconds(_leaveGrowTime);
                }
            }

            if (flowerStems != null)
            {
                StartCoroutine(ShrinkStem(flowerStems, _stemGrowTime));
                yield return new WaitForSeconds(_stemGrowTime);
            }
        }

        isGrowing = false;
        isGrown = false;
    }
    
    public void Started(PlayerInput playerInput)
    {
        Debug.Log("Flower Power Started");
        if (isGrowing)
            return;

        if (!isGrown)
        {
            StartCoroutine(GrowFlower());
            //animator.SetTrigger("Special");
        }
        else
        {
            StartCoroutine(ShrinkFlower());
        }
    }

    public void Canceled(PlayerInput playerInput)
    {
    }


    /*void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            StartCoroutine(ShrinkFlower());
        }
    }*/
}