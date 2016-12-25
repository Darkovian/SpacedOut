using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shieldScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.position = new Vector3(GameObject.Find("playerShip").transform.position.x + 1, GameObject.Find("playerShip").transform.position.y -.5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(collision.gameObject);
        Destroy(gameObject);
        GameObject.Find("playerShip").GetComponent<playerMovement>().hasShield = false;
    }
}
