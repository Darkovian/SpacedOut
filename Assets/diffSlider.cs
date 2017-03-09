using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class diffSlider : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (PlayerPrefs.GetFloat("Difficulty") >= 0.6 && PlayerPrefs.GetFloat("Difficulty") <= 1.5)
        {
            gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("Difficulty");
        }
        else
        {
            PlayerPrefs.SetFloat("Difficulty", 1.0f);
            gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("Difficulty");
        }
    }
	
	// Update is called once per frame
	void Update () {
        PlayerPrefs.SetFloat("Difficulty", gameObject.GetComponent<Slider>().value);
    }
}
