using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Food") {
            Destroy(collision.gameObject);
            missedCount++;
            UpdateMissedText();
            IncrementAngerMeter(0.1f); // Increase by 10%
            FindObjectOfType<ScoreManager>().ResetCombo(); // Reset the combo meter
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
        gameOverPanel.SetActive(true);       
        SoundManager.instance.StopBackgroundMusic(); // stop background music
        SoundManager.instance.PlayGameOver(); // play game over sound
        Time.timeScale = 0f; // pause the game
        
    }

    public void RestartGame() {
        Time.timeScale = 1f; // unpause the game
        SoundManager.instance.PlayBackgroundMusic();
        
        // reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
