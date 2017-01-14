using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class diffSlider : MonoBehaviour {

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("Difficulty");
    }
	
	// Update is called once per frame
	void Update () {
        PlayerPrefs.SetFloat("Difficulty", gameObject.GetComponent<Slider>().value);
    }
}
