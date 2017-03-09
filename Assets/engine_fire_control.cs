using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class engine_fire_control : MonoBehaviour {
    public float rotTime;
    float lastTime;

    // Use this for initialization
    void Start () {
        lastTime = Time.timeSinceLevelLoad;
	}
	
	// Update is called once per frame
	void Update () {
        float currTime = Time.timeSinceLevelLoad;

        if ((currTime - lastTime) > rotTime)
        {
            if (gameObject.GetComponent<SpriteRenderer>().flipX == true)
            {
                gameObject.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().flipX = true;
            }
            lastTime = Time.timeSinceLevelLoad;
        }

	}
}
