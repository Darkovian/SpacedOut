using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectSpawning : MonoBehaviour {
    public bool spawnEnabled = false;
    public float beginDelay;
    public float spread;
    public float spreadBonus;
    float timeOfLastObstacle;
    float timeSinceLastObstacle;
    float timeOfLastBonus;
    float timeSinceLastBonus;
    int die_roll;
    int die_roll2;
    private bool beginningDelay = false;

	// Use this for initialization
	void Start () {
        timeOfLastObstacle = 0;
        timeSinceLastObstacle = 0;
        timeOfLastObstacle = 0;
        timeSinceLastObstacle = 0;
    }
	
	// Update is called once per frame
	void Update () {
    
    if (beginningDelay == false && Time.timeSinceLevelLoad > beginDelay)
        {
            spawnEnabled = true;
            beginningDelay = true;
        }


    if (spawnEnabled && GameObject.Find("GameMaster").GetComponent<GameMaster>().bossBattle == false)
        {
            spawnStuff();
        }
        

	}

    public void spawnStuff()
    {
        //Obstacle Spawning Code

        timeSinceLastObstacle = Time.timeSinceLevelLoad - timeOfLastObstacle;
        if (timeSinceLastObstacle > (spread + Random.Range(-0.75f, 1.0f)))
        {
            // Debug.Log("Spawn Triggered!");
            die_roll = Random.Range(0, 11);
            die_roll2 = Random.Range(0, 5);
            if (die_roll < 6)
            {
                GameObject.Instantiate(Resources.Load("smallObstacle"));
            }
            if (die_roll > 6 && die_roll < 10)
            {
                GameObject.Instantiate(Resources.Load("mediumObstacle"));
                // GameObject.Instantiate(Resources.Load("fuelCan"));
            }
            if (die_roll == 10)
            {
                GameObject.Instantiate(Resources.Load("largeObstacle"));
            }
            timeOfLastObstacle = Time.timeSinceLevelLoad;
        }

        // Bonus Spawning Code

        timeSinceLastBonus = Time.timeSinceLevelLoad - timeOfLastBonus;
        if (timeSinceLastBonus > (spreadBonus + Random.Range(-0.75f, 2.0f)))
        {
            if (die_roll2 < 1)
            {
                GameObject.Instantiate(Resources.Load("coin1"));
            }
            if (GameObject.Find("playerShip").GetComponent<playerMovement>().currentFuel > (GameObject.Find("playerShip").GetComponent<playerMovement>().maxFuel * .7))
            {
                if (die_roll2 < 1)
                {
                    GameObject.Instantiate(Resources.Load("fuelCan"));
                }
            }
            else if (GameObject.Find("playerShip").GetComponent<playerMovement>().currentFuel > (GameObject.Find("playerShip").GetComponent<playerMovement>().currentFuel * .6))
            {
                if (die_roll2 < 3)
                {
                    GameObject.Instantiate(Resources.Load("fuelCan"));
                }
            }
            else if (GameObject.Find("playerShip").GetComponent<playerMovement>().currentFuel < (GameObject.Find("playerShip").GetComponent<playerMovement>().currentFuel * .45))
            {
                if (die_roll2 < 5)
                {
                    GameObject.Instantiate(Resources.Load("fuelCan"));
                }
            }
            timeOfLastBonus = Time.timeSinceLevelLoad;
        }
    }
}
