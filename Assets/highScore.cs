using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class highScore : MonoBehaviour {
    private float _highScore;

	// Use this for initialization
	void Start () {
        _highScore = PlayerPrefs.GetFloat("Highscore");
        if (GameObject.Find("GameMaster").GetComponent<GameMaster>().playerScore > _highScore)
        {
            PlayerPrefs.SetFloat("Highscore", GameObject.Find("GameMaster").GetComponent<GameMaster>().playerScore);
        }
        GameObject.Find("txt_Highscore").GetComponent<Text>().text = _highScore.ToString();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
