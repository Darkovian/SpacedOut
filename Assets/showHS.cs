using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class showHS : MonoBehaviour {

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Text>().text = PlayerPrefs.GetFloat("Highscore").ToString();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
