using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class AngerManager : MonoBehaviour {
    // public variables adjustable in Unity
    public GameObject player;
    public GameObject gameOverPanel; // UI panel to display when the game is over
    public Image angerMeterFill; // UI element for the anger meter fill
    public SpriteRenderer playerHeadSpriteRenderer; // player head
    public SpriteRenderer playerBodySpriteRenderer; // player body
    public TextMeshProUGUI missText; // UI text for showing missed food notification
    public TextMeshProUGUI angerText; // UI text for showing current anger level
    public TextMeshProUGUI finalScoreText; // UI text for displaying final score at game over
    public Color[] angerColours; // list of colours corresponding to different anger levels
    public TextMeshProUGUI[] rankTexts; // UI list of text boxes for showing the 10 rank levels
    public AudioClip successSound; // rank reveal sound effect
    public const int rankThreshold = 10000; // points needed for each rank
    public FoodSpawner foodSpawner;
 
    // variables for internal logic
    public float angerMeterValue = 0f; // current anger level
    private const float defaultAngerMeterValue = 0f;
    private const float angerIncrement = 0.1f; // anger increases by 10% for each miss
    private const float rankRevealInterval = 1f; // time interval between revealing ranks
    private const float unpauseTimeValue = 1f; // time scale value to unpause the game
    private const float pauseTimeValue = 0f; // time scale value to pause the game
    private const float rankPitchIncrement = 0.05f; // for increasing each rank sound effect pitch
    private const float maxAngerValue = 1f; // max value of the anger meter (100%)
    private const float maxAudioPitch = 1f; // max audio pitch (full volume)
    private const float missTextDuration = 0.3f;
    private const int minAngerIndex = 0; // index of lowest anger value
    private const int maxAngerIndex = 9; // index of highest anger value
    private const int angerMultiplier = 10; // multiplier for converting anger meter value to anger index
    private const int lowestRank = 1;
    private const int highestRank = 10;
    private UIManager uiManager; // to access boundary data
    private AudioSource audioSource;
    private GameController gameManager; // to access player data
    // list of anger level descriptions corresponding to the anger meter value
    private string[] angerLevels = { "Happy", "Pleased", "Content", "Neutral", 
        "Irritated", "Annoyed", "Frustrated", "Angry!", "Enraged!!", "Furious!!!" };

    private void Start() {
        // initialise references
        uiManager = FindObjectOfType<UIManager>();
        gameManager = FindObjectOfType<GameController>();
        foodSpawner = FindObjectOfType<FoodSpawner>();
    }

    private void Awake() {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // triggered when a food item enters the boundary collider (i.e. when food is missed)
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Food") {

            Food food = collision.GetComponent<Food>();

            // if the missed food is rotten, destroy it without increasing the anger meter
            if (food.powerUpType == Food.PowerUpType.Rotten) {
                foodSpawner.ReturnToPool(collision.gameObject);
                return;
            }
            
            // remove the missed food and show the miss notification
            foodSpawner.ReturnToPool(collision.gameObject);
            StartCoroutine(ShowMissText());
            IncrementAngerMeter(angerIncrement); // increase anger by 10%
        }
    }

    // display the "Miss" notification when food is missed
    private IEnumerator ShowMissText() {
        missText.gameObject.SetActive(true);
        yield return new WaitForSeconds(missTextDuration);
        missText.gameObject.SetActive(false);
    }

    // resets the anger meter to its initial state (i.e. level 0, "happy", bar empty)
    public void ResetAngerMeter() {
        angerMeterValue = defaultAngerMeterValue;
        angerMeterFill.fillAmount = angerMeterValue;
        angerMeterFill.color = Color.Lerp(Color.blue, Color.red, angerMeterValue);
        UpdateAngerText();
    }

    // increments the anger meter and checks if it has reached the maximum value
    private void IncrementAngerMeter(float increment) {
        float targetAngerValue = angerMeterValue + increment;
        // ensure the anger value doesn't exceed the max
        targetAngerValue = Mathf.Clamp(targetAngerValue, defaultAngerMeterValue, maxAngerValue); 

        // smoothly fill the anger meter bar
        StartCoroutine(SmoothFillAngerMeter(targetAngerValue));

        UpdatePlayerColours(); // update player colours based on the new anger value

        // check if the anger meter has reached its maximum value (game over)
        if (targetAngerValue >= maxAngerValue) {
            GameOver();
        }
    }

    // smoothly animate the filling of the anger meter bar
    private IEnumerator SmoothFillAngerMeter(float targetValue) {
        float initialFill = angerMeterFill.fillAmount;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            angerMeterFill.fillAmount = Mathf.Lerp(initialFill, targetValue, elapsed / duration);
            angerMeterFill.color = Color.Lerp(Color.blue, Color.red, angerMeterFill.fillAmount);
            yield return null;
        }

        // ensure fill amount is exactly the target value at the end
        angerMeterFill.fillAmount = targetValue;
        angerMeterValue = targetValue; // update the actual anger value after the smooth fill
        angerMeterFill.color = Color.Lerp(Color.blue, Color.red, angerMeterFill.fillAmount);
        UpdateAngerText();
    }

    // updates the player's colours (head and body) based on the current anger level
    private void UpdatePlayerColours() {
        int angerIndex = Mathf.Clamp(Mathf.FloorToInt(angerMeterValue * angerMultiplier), minAngerIndex, maxAngerIndex);
        playerBodySpriteRenderer.color = angerColours[angerIndex];
        playerHeadSpriteRenderer.sprite = gameManager.mouthClosedSprites[angerIndex];
    }  

    // updates the anger text on the anger meter, based on the current anger level
    private void UpdateAngerText() {
        if (angerMeterValue >= maxAngerValue) {
            angerText.text = null; // no text if the anger meter is full (game over)
        } else {
            int angerIndex = Mathf.Clamp(Mathf.FloorToInt(angerMeterValue * angerMultiplier), minAngerIndex, maxAngerIndex);
            angerText.text = angerLevels[angerIndex]; // display the appropriate anger level
        }
    }

    // Game Over: stop the game, show game over panel, reveal ranks
    private void GameOver() {
        RemoveAllFoodItems(); // remove any remaining food items from the scene
        RemoveAngerMeter(); // hide the anger meter
        gameOverPanel.SetActive(true); // show the game over UI       
        SoundManager.instance.StopBackgroundMusic(); // stop background music
        SoundManager.instance.PlayGameOver(); // play game over sound
        Time.timeScale = pauseTimeValue; // pause the game

        // display score
        int currentScore = uiManager.GetCurrentScore();
        finalScoreText.text = currentScore.ToString();
        StartCoroutine(RevealRanks(currentScore));
    }

    // reveal the player's rank based on their final score
    private IEnumerator RevealRanks(int finalScore) {
        // calculate the player's rank
        int achievedRank = Mathf.Clamp(highestRank - (finalScore / rankThreshold), lowestRank, highestRank);
        for (int i = rankTexts.Length - 1; i >= achievedRank - 1; i--) {
            rankTexts[i].gameObject.SetActive(true);
            
            // increase the pitch of the rank up sound for each rank revealed
            float pitch = maxAudioPitch + (rankPitchIncrement * (rankTexts.Length - 1 - i));
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(successSound);

            yield return new WaitForSecondsRealtime(rankRevealInterval);
        }
    }

    // restarts the game, reloads the current scene
    public void RestartGame() {
        // hide all rank texts
        foreach (var rankText in rankTexts) {
            rankText.gameObject.SetActive(false);
        }
        Time.timeScale = unpauseTimeValue; // unpause the game
        SoundManager.instance.SetBackgroundMusicPitch(maxAudioPitch); // reset background music pitch
        SoundManager.instance.PlayBackgroundMusic();
        
       SceneManager.LoadScene(SceneManager.GetActiveScene().name); // reload the game scene
    }

    // exits the game
    public void ExitGame() { 
        Application.Quit();
    }

    // removes all food items currently in the scene
    private void RemoveAllFoodItems() {
        GameObject[] foodItems = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject foodItem in foodItems) {
            Destroy(foodItem);
        }
    }

    // hides the anger meter UI 
    private void RemoveAngerMeter() {
    GameObject[] angerItems = GameObject.FindGameObjectsWithTag("AngerMeter");
        foreach (GameObject angerItem in angerItems) {
            angerItem.SetActive(false);
        }
    }
}