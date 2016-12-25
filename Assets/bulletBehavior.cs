using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletBehavior : MonoBehaviour {


    public float speed;

	// Use this for initialization
	void Start () {
        gameObject.transform.position = new Vector3(GameObject.Find("playerShip").transform.position.x + 1, GameObject.Find("playerShip").transform.position.y - .5f);
    }
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x + (1 * speed), gameObject.transform.position.y);
        if (gameObject.transform.position.x > 20)
        {
            Destroy(gameObject);
        }
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(collision.gameObject);
        Destroy(gameObject);
        GameObject.Find("playerShip").GetComponent<playerMovement>().playerScoreBonus = GameObject.Find("playerShip").GetComponent<playerMovement>().playerScoreBonus + 10;
    }
}
