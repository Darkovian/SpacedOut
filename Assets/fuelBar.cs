using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fuelBar : MonoBehaviour {
    float fuelRatio;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        fuelRatio = GameObject.Find("playerShip").GetComponent<playerMovement>().currentFuel / GameObject.Find("playerShip").GetComponent<playerMovement>().maxFuel;

        gameObject.GetComponent<Image>().fillAmount = fuelRatio;
	}
}
