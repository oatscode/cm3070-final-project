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

    private void IncrementAngerMeter(float increment) {
        angerMeterValue += increment;
        angerMeterFill.fillAmount = angerMeterValue;

        // Change color based on the fill amount
        angerMeterFill.color = Color.Lerp(Color.blue, Color.red, angerMeterValue);

        UpdateAngerText();
        UpdatePlayerColors();

        // Game over if anger hits 100%
        if (angerMeterValue >= 1f) {
            GameOver();
        }
    }

    public void DecrementAngerMeter(float decrement) {
        angerMeterValue -= decrement;
        angerMeterValue = Mathf.Clamp(angerMeterValue, 0, 1);
        angerMeterFill.fillAmount = angerMeterValue;

        // Change colour based on the fill amount
        angerMeterFill.color = Color.Lerp(Color.blue, Color.red, angerMeterValue);

        UpdateAngerText();
        UpdatePlayerColors();
    }

    private void UpdatePlayerColors() {
        // PlayerBody color from RGB(0,204,0) to RGB(255,0,0)
        Color playerBodyStartColour = new Color(0f / 255f, 204f / 255f, 0f / 255f);
        Color playerBodyEndColour = new Color(255f / 255f, 0f / 255f, 0f / 255f);
        playerBodySpriteRenderer.color = Color.Lerp(playerBodyStartColour, playerBodyEndColour, angerMeterValue);

        // Player color from RGB(0,255,255) to RGB(255,255,255)
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
        SoundManager.instance.StopBackgroundMusic(); // Stop background music
        SoundManager.instance.PlayGameOver(); // Play game over sound
        Time.timeScale = 0f; // Pause the game
        
    }

    public void RestartGame() {
        Time.timeScale = 1f; // Unpause the game
        SoundManager.instance.PlayBackgroundMusic();
        
        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
