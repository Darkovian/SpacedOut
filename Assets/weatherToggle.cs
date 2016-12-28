using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class weatherToggle : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (PlayerPrefs.GetInt("WeatherFX") == 1)
        {
            gameObject.GetComponent<Toggle>().isOn = true;
        }

        if (PlayerPrefs.GetInt("WeatherFX") == 0)
        {
            gameObject.GetComponent<Toggle>().isOn = false;
        }
    }
	
	// Update is called once per frame
	void Update () {
		if (gameObject.GetComponent<Toggle>().isOn)
        {
            PlayerPrefs.SetInt("WeatherFX", 1);
        }
        if (!gameObject.GetComponent<Toggle>().isOn)
        {
            PlayerPrefs.SetInt("WeatherFX", 0);
        }
    }
}
