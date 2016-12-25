using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.WeatherMaker;

public class Weather : MonoBehaviour {

    public WeatherMakerScript WeatherScript;


	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        WeatherScript.CurrentPrecipitation = WeatherScript.RainScript;
        WeatherScript.PrecipitationIntensity = .5f;
        WeatherScript.LightningScript.EnableLightning = true;
        WeatherScript.CloudScript.enabled = false;
        // WeatherScript.DayNightScript.TimeOfDay++;
    }
}
