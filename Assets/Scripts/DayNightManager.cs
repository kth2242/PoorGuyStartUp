using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    //Change it to control the duration of the day 
    private const float DAYDURATION = 10f;

    [SerializeField]
    private Light Sun;
    private float sunIntensityLvl = 1;

    [SerializeField]
    private Light pointLight;
    private float plIntensityLvl = 0;

    //Long term is for Day&Night, and lighting changes per short term
    WaitForSeconds longTerm = new WaitForSeconds(DAYDURATION);
    WaitForSeconds shortTerm = new WaitForSeconds(1f/DAYDURATION);

	// Use this for initialization
	void Start ()
    {
        //Starting from day to night
        StartCoroutine(startNightCycle());
	}
	
	// Update is called once per frame
	void Update ()
    {
        Sun.intensity = sunIntensityLvl;
        pointLight.intensity = plIntensityLvl;
    }

    //Cycle that starts from day to night
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

    //Cycle that starts from night to day
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
