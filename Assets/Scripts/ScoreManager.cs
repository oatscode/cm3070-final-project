using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    private int score = 0;
    private int consecutiveEaten = 0; 
    public BoundaryDestroyer boundaryDestroyer;
    public GameObject playerBody;
    private float screenHeight;

    private void Start() {
        UpdateScore();
        ResetCombo();

        // Calculate screen height in world units
        screenHeight = Camera.main.orthographicSize * 2;
    }

    public void AddScore(int points) {
        score += points;
        consecutiveEaten++;
        UpdateScore();
        UpdateCombo();

        // Increase PlayerBody Y scale
        Vector3 newScale = playerBody.transform.localScale;
        newScale.y += 0.1f;
        playerBody.transform.localScale = newScale;

        // Check for win condition
        if (playerBody.transform.localScale.y >= screenHeight) {
            Debug.Log("You Win!");
        }

        if (consecutiveEaten >= 3) {
            consecutiveEaten = 0;
            ResetCombo();
            boundaryDestroyer.DecrementAngerMeter(0.1f); // Decrease anger meter by 10%
        }
    }

    public void ResetCombo() {
        comboText.text = "Combo: ";
    }

    private void UpdateCombo() {
        comboText.text = "Combo: " + new string('O', consecutiveEaten);
    }

    private void UpdateScore() {
        scoreText.text = "Score: " + score;
    }
}