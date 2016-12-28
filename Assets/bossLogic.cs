using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossLogic : MonoBehaviour {
    float timeOfLastShot = 0;
    float timeSinceLastShot = 50;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // gameObject.transform.position = new Vector3(gameObject.transform.position.x + Random.Range(-1, 1), gameObject.transform.position.y + Random.Range(-1, 1));
        timeSinceLastShot = Time.timeSinceLevelLoad - timeOfLastShot;
        if (timeSinceLastShot > 1)
        {
            GameObject.Instantiate(Resources.Load("enemyBullet"));
            timeOfLastShot = Time.timeSinceLevelLoad;
        }
        GameObject.Find("playerShip").GetComponent<playerMovement>().currentFuel = GameObject.Find("playerShip").GetComponent<playerMovement>().maxFuel;

    }

    private void OnDestroy()
    {
        GameObject.Find("GameMaster").GetComponent<GameMaster>().bossBattle = false;
    }
}
