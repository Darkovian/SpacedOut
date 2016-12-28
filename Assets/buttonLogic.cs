using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonLogic : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void btnPlay()
    {
        SceneManager.LoadScene(1);
    }

    public void btnQuit()
    {
        Application.Quit();
    }

    public void btnRetry()
    {
        Destroy(GameObject.Find("GameMaster"));
        SceneManager.LoadScene(1);
    }

    public void btnSettings()
    {
        SceneManager.LoadScene(3);
    }

    public void btnBack()
    {
        SceneManager.LoadScene(0);
    }
}
