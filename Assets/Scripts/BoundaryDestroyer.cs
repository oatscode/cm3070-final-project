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
