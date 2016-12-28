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
        if (PlayerPrefs.GetInt("WeatherFX") == 1)
        {
            WeatherScript.enabled = true;
            WeatherScript.CurrentPrecipitation = WeatherScript.RainScript;
            WeatherScript.PrecipitationIntensity = PlayerPrefs.GetFloat("PrecipStr");
            WeatherScript.LightningScript.EnableLightning = true;
            WeatherScript.CloudScript.enabled = false;
            // WeatherScript.DayNightScript.TimeOfDay++;
        }
        else if (PlayerPrefs.GetInt("WeatherFX") == 0)
        {
            WeatherScript.enabled = false;
        }
    }
}
