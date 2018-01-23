using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    //Change it to control the duration of the day 
    private const float DAYDURATION = 10f;
    private const float PI = 3.14159f;

    [SerializeField]
    private Light Sun;
    private float sunIntensityLvl = 1;

    [SerializeField]
    private Light pointLight;
    private float plIntensityLvl = 0;

    [SerializeField]
    private GameObject timeImage;

    //Long term is for Day&Night, and lighting changes per short term
    WaitForSeconds longTerm = new WaitForSeconds(DAYDURATION);
    WaitForSeconds shortTerm = new WaitForSeconds(1f/DAYDURATION);

    // Use this for initialization
    void Start ()
    {
        //Starting from day to night
        StartCoroutine(startNightCycle());
        StartCoroutine(startTimeCycle());
    }
	
	// Update is called once per frame
	void Update ()
    {
        Sun.intensity = sunIntensityLvl;
        pointLight.intensity = plIntensityLvl;
        timeImage.transform.Rotate(0, 0, (2*PI*100) / (2*(DAYDURATION * (DAYDURATION + 1f))) * Time.deltaTime);
    }

    //Cycle that starts from day to night
    IEnumerator startNightCycle()
    {
        yield return longTerm;
        while (sunIntensityLvl > 0f && plIntensityLvl < 1f)
        {
            yield return longTerm;
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
            yield return longTerm;
            sunIntensityLvl += (1f / DAYDURATION);
            plIntensityLvl -= (1f / DAYDURATION);
        }
        yield return StartCoroutine(startNightCycle());
    }

    IEnumerator startTimeCycle()
    {

        yield return shortTerm;
        //timeImage.transform.Rotate(0, 0, PI*(DAYDURATION+(1f/DAYDURATION)) * Time.deltaTime);

        yield return StartCoroutine(startTimeCycle());
    }
}
