using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class BoundaryDestroyer : MonoBehaviour {
    public TextMeshProUGUI missedText;
    private int missedCount = 0;
    public Image angerMeterFill;
    private float angerMeterValue = 0f; 
    public TextMeshProUGUI angerText;
    private string[] angerLevels = { "Happy", "Annoyed", "Frustrated", "Angry", "Furious", "Enraged", "Livid", "Seething", "Irate", "Infuriated" };
    public GameObject gameOverPanel;

    public SpriteRenderer playerSpriteRenderer;
    public SpriteRenderer playerBodySpriteRenderer;

    public TextMeshProUGUI finalScoreText;
    private ScoreManager scoreManager;

    public TextMeshProUGUI[] rankTexts;
    public AudioClip successSound;
    private AudioSource audioSource;
    public int rankThreshold = 10000; // points needed for each rank

    private void Start() {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    private void Awake() {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Food") {
            Destroy(collision.gameObject);
            missedCount++;
            UpdateMissedText();
            IncrementAngerMeter(0.1f); // increase by 10%
            FindObjectOfType<ScoreManager>().ResetCombo(); // reset the combo meter
        }
    }

    public void ResetAngerMeter() {
        angerMeterValue = 0f;
        angerMeterFill.fillAmount = angerMeterValue;
        angerMeterFill.color = Color.Lerp(Color.blue, Color.red, angerMeterValue);
        UpdateAngerText();
    }

    private void IncrementAngerMeter(float increment) {
        angerMeterValue += increment;
        angerMeterFill.fillAmount = angerMeterValue;

        // change colour based on the fill amount
        angerMeterFill.color = Color.Lerp(Color.blue, Color.red, angerMeterValue);

        UpdateAngerText();
        UpdatePlayerColors();

        // game over if anger hits 100%
        if (angerMeterValue >= 1f) {
            GameOver();
        }
    }

    public void DecrementAngerMeter(float decrement) {
        angerMeterValue -= decrement;
        angerMeterValue = Mathf.Clamp(angerMeterValue, 0, 1);
        angerMeterFill.fillAmount = angerMeterValue;

        // change colour based on the fill amount
        angerMeterFill.color = Color.Lerp(Color.blue, Color.red, angerMeterValue);

        UpdateAngerText();
        UpdatePlayerColors();
    }

    private void UpdatePlayerColors() {
        Color playerBodyStartColour = new Color(0f / 255f, 204f / 255f, 0f / 255f);
        Color playerBodyEndColour = new Color(255f / 255f, 0f / 255f, 0f / 255f);
        playerBodySpriteRenderer.color = Color.Lerp(playerBodyStartColour, playerBodyEndColour, angerMeterValue);

        Color playerStartColour = new Color(0f / 255f, 255f / 255f, 255f / 255f);
        Color playerEndColour = new Color(255f / 255f, 255f / 255f, 255f / 255f);
        playerSpriteRenderer.color = Color.Lerp(playerStartColour, playerEndColour, angerMeterValue);
    }  

    private void UpdateAngerText() {
        if (angerMeterValue >= 1f) {
            angerText.text = "";
        } else {
            int angerIndex = Mathf.Clamp(Mathf.FloorToInt(angerMeterValue * 10), 0, 9);
            angerText.text = angerLevels[angerIndex];
        }
    }

    private void UpdateMissedText() {
        missedText.text = "Missed: " + missedCount;
    }

    private void GameOver() {
        RemoveAllFoodItems();
        RemoveAngerMeter();
        gameOverPanel.SetActive(true);       
        SoundManager.instance.StopBackgroundMusic(); // stop background music
        SoundManager.instance.PlayGameOver(); // play game over sound
        Time.timeScale = 0f; // pause the game

        // display score
        int currentScore = scoreManager.GetCurrentScore();
        finalScoreText.text = currentScore.ToString();
        StartCoroutine(RevealRanks(currentScore));
    }

    private IEnumerator RevealRanks(int finalScore) {
        int achievedRank = Mathf.Clamp(10 - (finalScore / rankThreshold), 1, 10);
        // Debug.Log("achievedRank " + achievedRank);
        // Debug.Log("rankTexts.Length - 1 " + (rankTexts.Length - 1));
        // Debug.Log("rankThreshold " + achievedRank);
        for (int i = rankTexts.Length - 1; i >= achievedRank - 1; i--) {
            rankTexts[i].gameObject.SetActive(true);
            
            // increase pitch of rank up sound
            float pitch = 1f + (0.03f * (rankTexts.Length - 1 - i));
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(successSound);

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    public void RestartGame() {
        foreach (var rankText in rankTexts) {
            rankText.gameObject.SetActive(false);
        }
        Time.timeScale = 1f; // unpause the game
        SoundManager.instance.SetBackgroundMusicPitch(1f);
        SoundManager.instance.PlayBackgroundMusic();
        
       SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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