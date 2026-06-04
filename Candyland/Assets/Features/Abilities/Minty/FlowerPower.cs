using UnityEngine;
using System.Collections;
using BSOAP.Variables;
using System.Collections.Generic;

public class FlowerPower : MonoBehaviour
{
    [SerializeField] private List<Transform> flowerLeaves = new List<Transform>();
    [SerializeField] private List<Transform> flowerStems = new List<Transform>();
    [SerializeField] private List<Transform> flowerPetals = new List<Transform>();

    [SerializeField] private FloatVariable growSpeed;

    void Start()
    {
        foreach (var stem in flowerStems)
        {
            stem.localScale = new Vector3(1, 0, 1);
        }

        foreach (var leaf in flowerLeaves)
        {
            leaf.localScale = Vector3.zero;
        }

        foreach (var petal in flowerPetals)
        {
            petal.localScale = Vector3.zero;
        }
        
        //StartCoroutine(GrowFlower());
    }

    IEnumerator GrowPart(Transform part, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        float timer = 0f;

        while(timer < duration)
        {
            timer += Time.deltaTime * growSpeed.Value;

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

        while(timer < duration)
        {
            timer += Time.deltaTime * growSpeed.Value;

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
            timer += Time.deltaTime * growSpeed.Value;

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

        while(timer < duration)
        {
            timer += Time.deltaTime * growSpeed.Value;

            float time = timer/duration;

            stem.localScale = Vector3.Lerp(startScale, endScale, time);


            yield return null;
        }

        stem.localScale = endScale;   
    }

    public IEnumerator GrowFlower()
    {
        if(flowerStems != null)
        {         
            for (int i = 0; i < flowerStems.Count; i++)
            {
                StartCoroutine(GrowStem(flowerStems[i], 3f));
            }
        }
        
        if(flowerLeaves != null)
        {           
            yield return new WaitForSeconds(0.6f);
            for (int i = 0; i < flowerLeaves.Count; i++)
            {
                StartCoroutine(GrowPart(flowerLeaves[i], 1.2f));
            }
        }

        if(flowerPetals != null)
        {           
            yield return new WaitForSeconds(0.3f);
            for (int i = 0; i < flowerPetals.Count; i++)
            {
                StartCoroutine(GrowPart(flowerPetals[i], 1.7f));
            }
        }
    }

    public IEnumerator ShrinkFlower()
    {
        if(flowerPetals != null)
        {           
            for (int i = 0; i < flowerPetals.Count; i++)
            {
                StartCoroutine(ShrinkPart(flowerPetals[i], 1.7f));
            }
        }
        
        if(flowerLeaves != null)
        {            
            yield return new WaitForSeconds(0.3f);
            for (int i = 0; i < flowerLeaves.Count; i++)
            {
                StartCoroutine(ShrinkPart(flowerLeaves[i], 1.2f));
            }
        }

        if(flowerStems != null)
        {           
            yield return new WaitForSeconds(0.6f);
            for (int i = 0; i < flowerStems.Count; i++)
            {
                StartCoroutine(ShrinkStem(flowerStems[i], 3f));
            }
        }
    }
    

    /*void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            StartCoroutine(ShrinkFlower());
        }
    }*/
}
