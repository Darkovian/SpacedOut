using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;

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
    public AudioClip laserSound;
    public AudioClip pickupSound;
    public AudioClip deathSound;
    public bool debugMovement;
    public int adCountValue;


    // Use this for initialization
    void Start () {
        isAlive = true;
        canMove = true;
        currentFuel = maxFuel;
        GameObject.Find("txtFuel").GetComponent<Text>().text = maxFuel.ToString();

        if (!(PlayerPrefs.GetInt("adCount") >= 0 && PlayerPrefs.GetInt("adCount") <= 3))
        {
            PlayerPrefs.SetFloat("adCount", 0);
        }

    }
	
	// Update is called once per frame
	void Update () {
        if (isAlive)
        {
            
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
            if (PlayerPrefs.GetInt("adCount") == adCountValue)
            {
                PlayerPrefs.SetFloat("adCount", 0);
                showAd();
            }
            else
            {
                PlayerPrefs.SetInt("adCount", PlayerPrefs.GetInt("adCount") + 1);
            }
            SceneManager.LoadScene(2);
        }


	}

    void showAd()
    {
        if (Advertisement.IsReady())
        {
            Advertisement.Show();
        }
    }

    public void shoot()
    {
        canMove = false;
        GameObject.Instantiate(Resources.Load("bullet"));
        gameObject.transform.position = lastKnownPos;
        gameObject.GetComponent<AudioSource>().clip = laserSound;
        gameObject.GetComponent<AudioSource>().Play();

    }

    void handleFuel()
    {
        currentFuel = currentFuel - (1 * fuelMult);
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
        playerScore = (playerScoreBase + playerScoreBonus) * PlayerPrefs.GetFloat("Difficulty");
        GameObject.Find("GameMaster").GetComponent<GameMaster>().playerScore = playerScore;
    }

    void movementControl()
    {

        if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x < 2 && canMove && Input.touchCount > 0)
        {
            Touch moveTouch = Input.GetTouch(0);
            gameObject.transform.position = new Vector3(-5.5f, Camera.main.ScreenToWorldPoint(moveTouch.position).y, 0);
        }

        // Testing Movement Code
        if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x < 2 && canMove && debugMovement == true)
        {
            lastKnownPos = gameObject.transform.position;
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
            gameObject.GetComponent<AudioSource>().clip = pickupSound;
            gameObject.GetComponent<AudioSource>().Play();
        }

        if (collision.gameObject.name.Contains("fuel"))
        {
            playerScoreBonus = playerScoreBonus + 30;
            currentFuel = currentFuel + 45;
            Destroy(collision.gameObject);
            gameObject.GetComponent<AudioSource>().clip = pickupSound;
            gameObject.GetComponent<AudioSource>().Play();
        }
        if (collision.gameObject.name.Contains("Obstacle") || collision.gameObject.name.Contains("enemyBullet"))
        {
            isAlive = false;
        }
    }
}
