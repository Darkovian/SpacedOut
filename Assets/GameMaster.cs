using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour {

    public float playerScore;
    public bool bossBattle = false;
    bool doOnce = false;

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
        /*if (playerScore > 1000 && doOnce == false)
        {
            doOnce = true;
            bossBattle = true;
            GameObject.Instantiate(Resources.Load("boss1"));
        }*/
	}

}
