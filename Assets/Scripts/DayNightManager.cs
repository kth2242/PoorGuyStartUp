using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    private const float DAYDURATION = 5f;

    [SerializeField]
    private Light Sun;
    private float sunIntensityLvl = 1;
    [SerializeField]
    private Light pointLight;
    private float plIntensityLvl = 0;

    WaitForSeconds longTerm = new WaitForSeconds(DAYDURATION);
    WaitForSeconds shortTerm = new WaitForSeconds(DAYDURATION);

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(startNightCycle());
	}
	
	// Update is called once per frame
	void Update ()
    {
        Sun.intensity = sunIntensityLvl;
        pointLight.intensity = plIntensityLvl;
    }

    IEnumerator startNightCycle()
    {
        yield return longTerm;
        while (sunIntensityLvl > 0f && plIntensityLvl < 1f)
        {
            yield return shortTerm;
            sunIntensityLvl -= (1f / DAYDURATION);
            plIntensityLvl += (1f / DAYDURATION);
        }
        yield return StartCoroutine(startDayCycle());
    }
 
    IEnumerator startDayCycle()
    {
        yield return longTerm;
        while (sunIntensityLvl < 1f && plIntensityLvl > 0f)
        {
            yield return shortTerm;
            sunIntensityLvl += (1f / DAYDURATION);
            plIntensityLvl -= (1f / DAYDURATION);
        }
        yield return StartCoroutine(startNightCycle());
    }
}
