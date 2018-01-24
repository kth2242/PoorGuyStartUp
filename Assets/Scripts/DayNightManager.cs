using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    //Change it to control the duration of the day 
    private const float DAYDURATION = 1f;

    [SerializeField]
    private Light Sun;
    private float sunIntensityLvl = 1;

    [SerializeField]
    private Light pointLight;
    private float plIntensityLvl = 0;

    [SerializeField]
    private GameObject timeImage;

    //Long term is for Day&Night and lighting changes
    WaitForSeconds longTerm = new WaitForSeconds(DAYDURATION);

    //Total duration of a day
    private const float dayImgCycle = (DAYDURATION * (DAYDURATION + 1f)) * 2f;
    private const float DEGREES_PER_SECOND = 360 / dayImgCycle;

    // Use this for initialization
    void Start ()
    {
        Time.timeScale = 1f;
        //Starting from day to night
        StartCoroutine(startNightCycle());
    }
	
	// Update is called once per frame
	void Update ()
    {
        Sun.intensity = sunIntensityLvl;
        pointLight.intensity = plIntensityLvl;
        //timeImage.transform.Rotate(0, 0, degreeRotation * Time.deltaTime);
        timeImage.transform.localRotation = Quaternion.Euler(0, 0, Time.fixedTime * DEGREES_PER_SECOND);
    }

    //Cycle that starts from day to night
    IEnumerator startNightCycle()
    {
        yield return longTerm;
        while (sunIntensityLvl >= (1f/DAYDURATION))
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
        while (plIntensityLvl >= (1f / DAYDURATION))
        {
            yield return longTerm;
            sunIntensityLvl += (1f / DAYDURATION);
            plIntensityLvl -= (1f / DAYDURATION);
        }
        yield return StartCoroutine(startNightCycle());
    }

}
