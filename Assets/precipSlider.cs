using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class precipSlider : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (PlayerPrefs.GetFloat("PrecipStr") >= 0 && PlayerPrefs.GetFloat("PrecipStr") <= 1)
        {
            gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("PrecipStr");
        }
        else
        {
            PlayerPrefs.SetFloat("PrecipStr", 0.5f);
            gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("PrecipStr");
        }
	}
	
	// Update is called once per frame
	void Update () {
        PlayerPrefs.SetFloat("PrecipStr", gameObject.GetComponent<Slider>().value);
    }
}
