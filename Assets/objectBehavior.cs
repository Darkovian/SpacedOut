using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectBehavior : MonoBehaviour {

    public float speed;
    public float rot;

	// Use this for initialization
	void Start () {
        gameObject.transform.position = new Vector3(9.6f, Random.Range(-4.5f, 4.5f));
        gameObject.GetComponent<Rigidbody2D>().AddTorque(rot);
	}

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x - (1 * speed), gameObject.transform.position.y);

        if (gameObject.transform.position.x < -10)
        {
            Destroy(gameObject);
        }
    }


}
