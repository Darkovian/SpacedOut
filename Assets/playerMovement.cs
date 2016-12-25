﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class playerMovement : MonoBehaviour {

    public float speed;
    public float playerScore;
    float playerScoreBase;
    public float playerScoreBonus = 0;
    public bool isAlive;
    float timeSinceLastShield = 5;
    float timeOfLastShield = 0;
    public int shieldCD;
    public bool hasShield = false;
    public float maxFuel;
    public float currentFuel;
    public float fuelMult;
    public bool canMove = true;
    float timeOfLastShot = 0;
    float timeSinceLastShot = 20;
    public float shootLock;
    public float shootCD;
    Vector3 lastKnownPos;
    

	// Use this for initialization
	void Start () {
        isAlive = true;
        canMove = true;
        currentFuel = maxFuel;
        GameObject.Find("txtFuel").GetComponent<Text>().text = maxFuel.ToString();
	}
	
	// Update is called once per frame
	void Update () {
        if (isAlive)
        {
            lastKnownPos = gameObject.transform.position;
            timeSinceLastShield = Time.timeSinceLevelLoad - timeOfLastShield;
            timeSinceLastShot = Time.timeSinceLevelLoad - timeOfLastShot;

            if (!canMove)
            {
                gameObject.transform.position = lastKnownPos;
            }

            if (!canMove && timeSinceLastShot > shootLock)
            {
                canMove = true;
            }

            if (canMove)
            {
                movementControl();
            }

            scoreAccumulation();
            handleFuel();
            GameObject.Find("txt_Score").GetComponent<Text>().text = playerScore.ToString();

        }

        if (!isAlive)
        {
            SceneManager.LoadScene(2);
        }


	}

    public void shoot()
    {
        canMove = false;
        gameObject.transform.position = lastKnownPos;
        GameObject.Instantiate(Resources.Load("bullet"));
    }

    void handleFuel()
    {
        currentFuel = currentFuel - (1 * fuelMult);
        GameObject.Find("txt_Fuel").GetComponent<Text>().text = currentFuel.ToString();
        if (currentFuel == 0)
        {
            isAlive = false;
        }

        if (currentFuel > 150)
        {
            currentFuel = 150;
        }
    }

    void scoreAccumulation()
    {
        playerScoreBase = Time.timeSinceLevelLoad;
        playerScore = playerScoreBase + playerScoreBonus;
        GameObject.Find("GameMaster").GetComponent<GameMaster>().playerScore = playerScore;
    }

    void movementControl()
    {
        if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x < 2 && canMove)
        {
            gameObject.transform.position = new Vector3(-5.5f, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);
        }
        
        
        /* if (Input.GetKey(KeyCode.W) == true)
        {
            if (gameObject.transform.position.y <= 4.5f)
            {
                gameObject.transform.position = new Vector3(-5.5f, gameObject.transform.position.y + (1 * speed));
            }
        }

        if (Input.GetKey(KeyCode.S) == true)
        {
            if (gameObject.transform.position.y >= -4.5f)
            {
                gameObject.transform.position = new Vector3(-5.5f, gameObject.transform.position.y - (1 * speed));
            }
        } */
    }

    public void enableShield()
    {
        if (timeSinceLastShield >= shieldCD && hasShield == false)
        {
            GameObject.Instantiate(Resources.Load("shipShield"));
            timeOfLastShield = Time.timeSinceLevelLoad;
            hasShield = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("coin"))
        {
            playerScoreBonus = playerScoreBonus + 100;
            currentFuel = currentFuel + 10;
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.name.Contains("fuel"))
        {
            playerScoreBonus = playerScoreBonus + 30;
            currentFuel = currentFuel + 45;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.name.Contains("Obstacle"))
        {
            isAlive = false;
        }
    }
}
