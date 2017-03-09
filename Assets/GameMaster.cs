using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour {

    public float playerScore;
    public bool bossBattle = false;
    bool doOnce = false;
    public AudioClip playerDeath;

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
        // BOSS BATTLE LGOIC
        /*if (playerScore > 1000 && doOnce == false)
        {
            doOnce = true;
            bossBattle = true;
            GameObject.Instantiate(Resources.Load("boss1"));
        }*/

        if (GameObject.Find("playerShip").GetComponent<playerMovement>().isAlive == false)
        {
            gameObject.GetComponent<AudioSource>().clip = playerDeath;
            gameObject.GetComponent<AudioSource>().Play();
        }
	}

}
