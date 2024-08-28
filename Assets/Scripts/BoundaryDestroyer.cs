using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BoundaryDestroyer : MonoBehaviour {
    public GameObject player;
    public GameObject gameOverPanel;
    public Image angerMeterFill;
    public float angerMeterValue = 0f; 
    private const float angerIncrement = 0.1f;
    private const float rankRevealInterval = 1f;
    private const float unpauseTimeValue = 1f;
    private const float pauseTimeValue = 0f;
    private const float rankPitchIncrement = 0.05f;
    private const float maxAngerValue = 1f;
    private const float maxAudioPitch = 1f;
    private const int minAngerIndex = 0;
    private const int maxAngerIndex = 9;
    private const int angerMultiplier = 10;
    private const int lowestRank = 1;
    private const int highestRank = 10;
    public SpriteRenderer playerHeadSpriteRenderer;
    public SpriteRenderer playerBodySpriteRenderer;
    public TextMeshProUGUI missText;
    public TextMeshProUGUI angerText;
    public TextMeshProUGUI finalScoreText;
    private ScoreManager scoreManager;
    public Color[] angerColours; 
    public TextMeshProUGUI[] rankTexts;
    public AudioClip successSound;
    private AudioSource audioSource;
    private PlayerController playerController;
    public const int rankThreshold = 10000; // points needed for each rank
    private string[] angerLevels = { "Happy", "Pleased", "Content", "Neutral", 
        "Irritated", "Annoyed", "Frustrated", "Angry!", "Enraged!!", "Furious!!!" };

    private void Start() {
        scoreManager = FindObjectOfType<ScoreManager>();
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Awake() {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update() {
        // missText.transform.position = new Vector3(missText.transform.position.x, player.transform.position.y + 1.5f, missText.transform.position.z);

        // angerText.transform.position = new Vector3(angerText.transform.position.x, player.transform.position.y - 1.5f, angerText.transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Food") {

            Food food = collision.GetComponent<Food>();

            // check if the missed food is rotten
            if (food.powerUpType == Food.PowerUpType.Rotten) {
                // don't increment the anger meter for rotten food
                Destroy(collision.gameObject);
                return;
            }
            
            Destroy(collision.gameObject);

            StartCoroutine(ShowMissText());

            IncrementAngerMeter(angerIncrement); // increase by 10%
        }
    }

    private IEnumerator ShowMissText() {
        missText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        missText.gameObject.SetActive(false);
    }

    public void ResetAngerMeter() {
        angerMeterValue = 0f;
        angerMeterFill.fillAmount = angerMeterValue;
        angerMeterFill.color = Color.Lerp(Color.blue, Color.red, angerMeterValue);
        UpdateAngerText();
    }

    private void IncrementAngerMeter(float increment) {
        float targetAngerValue = angerMeterValue + increment;
        targetAngerValue = Mathf.Clamp(targetAngerValue, 0f, maxAngerValue); // ensure the value doesn't exceed the max

        StartCoroutine(SmoothFillAngerMeter(targetAngerValue));

        UpdatePlayerColours();

        // check for game over condition
        if (targetAngerValue >= maxAngerValue) {
            GameOver();
        }
    }

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


    private void UpdatePlayerColours() {
        int angerIndex = Mathf.Clamp(Mathf.FloorToInt(angerMeterValue * angerMultiplier), minAngerIndex, maxAngerIndex);
        playerBodySpriteRenderer.color = angerColours[angerIndex];
        playerHeadSpriteRenderer.sprite = playerController.mouthClosedSprites[angerIndex];
    }  

    private void UpdateAngerText() {
        if (angerMeterValue >= maxAngerValue) {
            angerText.text = "";
        } else {
            int angerIndex = Mathf.Clamp(Mathf.FloorToInt(angerMeterValue * 10), 0, 9);
            angerText.text = angerLevels[angerIndex];
        }
    }

    private void GameOver() {
        RemoveAllFoodItems();
        RemoveAngerMeter();
        gameOverPanel.SetActive(true);       
        SoundManager.instance.StopBackgroundMusic(); // stop background music
        SoundManager.instance.PlayGameOver(); // play game over sound
        Time.timeScale = pauseTimeValue; // pause the game

        // display score
        int currentScore = scoreManager.GetCurrentScore();
        finalScoreText.text = currentScore.ToString();
        StartCoroutine(RevealRanks(currentScore));
    }

    private IEnumerator RevealRanks(int finalScore) {
        int achievedRank = Mathf.Clamp(highestRank - (finalScore / rankThreshold), lowestRank, highestRank);
        for (int i = rankTexts.Length - 1; i >= achievedRank - 1; i--) {
            rankTexts[i].gameObject.SetActive(true);
            
            // increase pitch of rank up sound
            float pitch = maxAudioPitch + (rankPitchIncrement * (rankTexts.Length - 1 - i));
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(successSound);

            yield return new WaitForSecondsRealtime(rankRevealInterval);
        }
    }

    public void RestartGame() {
        foreach (var rankText in rankTexts) {
            rankText.gameObject.SetActive(false);
        }
        Time.timeScale = unpauseTimeValue; // unpause the game
        SoundManager.instance.SetBackgroundMusicPitch(maxAudioPitch);
        SoundManager.instance.PlayBackgroundMusic();
        
       SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame() { 
        Application.Quit();
    }

    private void RemoveAllFoodItems() {
        GameObject[] foodItems = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject foodItem in foodItems) {
            Destroy(foodItem);
        }
    }

    private void RemoveAngerMeter() {
    GameObject[] angerItems = GameObject.FindGameObjectsWithTag("AngerMeter");
        foreach (GameObject angerItem in angerItems) {
            angerItem.SetActive(false);
        }
    }
}