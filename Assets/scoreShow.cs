using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scoreShow : MonoBehaviour {

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Text>().text = GameObject.Find("GameMaster").GetComponent<GameMaster>().playerScore.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
