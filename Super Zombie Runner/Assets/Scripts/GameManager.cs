using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject playerPrefab;
    public Text continueText;
    public Text scoreText;

    private float timeElapsed = 0f;
    private float bestTime = 0f;
    private float blinkTime = 0f;
    private bool blink;
    private bool gameStarted;
    private TimeManager timeManager;
    private GameObject player;
    private GameObject floor;
    private Spawner spawner;
    private bool newHighScore;

    void Awake()
    {
        floor = GameObject.Find("Foreground");
        spawner = GameObject.Find("Spawner").GetComponent<Spawner>();
        timeManager = GetComponent<TimeManager>();
    }
    
	void Start () {
        var floorHeight = floor.transform.localScale.y;

        var pos = floor.transform.position;
        pos.x = 0;
        pos.y = -(Screen.height/PixelPerfectCamera.pixelsToUnits)/2 + (floorHeight/2);
        floor.transform.position = pos;

        spawner.active = false;

        Time.timeScale = 0;

        continueText.text = "Press any button to start";

        bestTime = PlayerPrefs.GetFloat("bestTime");
    }
	
	void Update () {
	    if (!gameStarted && Time.timeScale == 0)
        {
            if (Input.anyKeyDown)
            {
                timeManager.ManipulateTime(1, 1f);
                ResetGame();
            }
        }

        if (!gameStarted)
        {
            blinkTime++;
            if(blinkTime % 40 == 0)
            {
                blink = !blink;
            }

            continueText.canvasRenderer.SetAlpha(blink ? 0 : 1);

            var textColor = newHighScore ? "#FF0" : "#FFF";

            scoreText.text = "TIME: " + formatTime(timeElapsed) + "\n<color=" + textColor +">BEST: " + formatTime(bestTime) + "</color>";
        } else
        {
            timeElapsed += Time.deltaTime;
            scoreText.text = "TIME: " + formatTime(timeElapsed);
        }
	}

    void OnPlayerKilled()
    {
        spawner.active = false;
        gameStarted = false;

        var playerDestroyScript = player.GetComponent<DestroyOffscreen>();
        playerDestroyScript.DestroyCallback -= OnPlayerKilled;

        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        timeManager.ManipulateTime(0, 5.5f);

        continueText.text = "Press any button to restart";

        if(timeElapsed > bestTime)
        {
            bestTime = timeElapsed;
            PlayerPrefs.SetFloat("bestTime", bestTime);
            newHighScore = true;
        }
    }

    void ResetGame()
    {
        spawner.active = true;
        gameStarted = true;

        player = GameObjectUtil.Instantiate(playerPrefab, new Vector3(0, (Screen.height / PixelPerfectCamera.pixelsToUnits) / 2 + 100, 0));

        var playerDestroyScript = player.GetComponent<DestroyOffscreen>();
        playerDestroyScript.DestroyCallback += OnPlayerKilled;

        continueText.canvasRenderer.SetAlpha(0);

        timeElapsed = 0;
    }

    string formatTime(float ms)
    {
        TimeSpan t = TimeSpan.FromSeconds(ms);
        return string.Format("{0:D2}:{1:D2}:{2:D3}", t.Minutes, t.Seconds, t.Milliseconds);
    }
}
