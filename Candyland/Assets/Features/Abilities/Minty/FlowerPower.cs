using UnityEngine;
using System.Collections;
using BSOAP.Variables;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Net.NetworkInformation;
using System;


[Serializable]
public class FlowerSection
{
    public List<Transform> flowerStems = new List<Transform>();
    public List<Transform> flowerLeaves = new List<Transform>();
    public List<Transform> flowerPetals = new List<Transform>();
}
public class FlowerPower : MonoBehaviour, IInteractable
{
    [SerializeField] private List<FlowerSection> flowerSections;
    [SerializeField] private float _stemGrowTime;
    [SerializeField] private float _leaveGrowTime;
    [SerializeField] private float _petalsGrowTime;

    private bool isGrowing = false;
    private bool isGrown = false;

    void Start()
    {
        foreach(var section in flowerSections)
        {
            foreach (var stem in section.flowerStems)
            {
                stem.localScale = new Vector3(1, 0, 1);
                stem.gameObject.SetActive(false);
            }

            foreach (var leaf in section.flowerLeaves)
            {
                leaf.localScale = Vector3.zero;
            }

            foreach (var petal in section.flowerPetals)
            {
                petal.localScale = Vector3.zero;
            }
        }
    }

    IEnumerator GrowPart(Transform part, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        float timer = 0f;

        while(timer < duration)
        {
            timer += Time.deltaTime;

            float time = timer/duration;

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

            float time = timer/duration;

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

        while(timer < duration)
        {
            timer += Time.deltaTime;

            float time = timer/duration;

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

            float time = timer/duration;

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


        foreach(var section in flowerSections)
        {
            var flowerStems = section.flowerStems;
            var flowerLeaves = section.flowerLeaves;
            var flowerPetals = section.flowerPetals;

            if (flowerStems != null)
            {
                for (int i = 0; i < flowerStems.Count; i++)
                {
                    StartCoroutine(GrowStem(flowerStems[i], _stemGrowTime));
                    yield return new WaitForSeconds(_stemGrowTime);
                }

            }

            if (flowerLeaves != null)
            {
                for (int i = 0; i < flowerLeaves.Count; i++)
                {
                    StartCoroutine(GrowPart(flowerLeaves[i], _leaveGrowTime));
                    yield return new WaitForSeconds(_leaveGrowTime);
                }

            }

            if (flowerPetals != null)
            {
                for (int i = 0; i < flowerPetals.Count; i++)
                {
                    StartCoroutine(GrowPart(flowerPetals[i], _petalsGrowTime));
                    yield return new WaitForSeconds(_petalsGrowTime);
                }
            }

        }

        isGrowing = false;
    }

    public IEnumerator ShrinkFlower()
    {
        isGrowing = true;

        foreach (var section in flowerSections)
        {
            var flowerStems = section.flowerStems;
            var flowerLeaves = section.flowerLeaves;
            var flowerPetals = section.flowerPetals;

            if (flowerStems != null)
            {
                for (int i = 0; i < flowerStems.Count; i++)
                {
                    StartCoroutine(ShrinkPart(flowerPetals[i], _stemGrowTime));
                }
                yield return new WaitForSeconds(_stemGrowTime / 2);
            }

            if (flowerLeaves != null)
            {
                for (int i = 0; i < flowerLeaves.Count; i++)
                {
                    StartCoroutine(ShrinkPart(flowerLeaves[i], _leaveGrowTime));
                }
                yield return new WaitForSeconds(_leaveGrowTime);
            }

            if (flowerPetals != null)
            {
                for (int i = 0; i < flowerPetals.Count; i++)
                {
                    StartCoroutine(ShrinkStem(flowerPetals[i], _petalsGrowTime));
                }
                yield return new WaitForSeconds(_petalsGrowTime);
            }

        }
        isGrowing = false;
        isGrown = false;
    }

    public void Started(PlayerInput playerInput)
    {
        if (isGrowing)
            return;

        if (!isGrown)
        {
            StartCoroutine(GrowFlower());
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
