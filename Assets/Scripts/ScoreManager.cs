using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI debugText;
    public GameObject uiCanvas;
    private int score = 0;
    private int consecutiveEaten = 0;
    private float minBodyHeightScale = 0.66f;
    private float maxBodyHeightScale = 6.8f;
    private int level = 1; 
    public BoundaryDestroyer boundaryDestroyer;
    public GameObject playerBody;
    public PlayerController playerController;
    public FoodSpawner foodSpawner;

    private float pointMultiplier = 1f;

    private void Start() {
        UpdateScore();
        ResetCombo();
        UpdateLevelText();
    }

    private void UpdateLevelText() {
        levelText.text = "Level: " + level;
    }

    public void AddScore(int basePoints) {
        int points = Mathf.RoundToInt(basePoints * pointMultiplier);
        if (level > 1) {
            points = Mathf.CeilToInt(points * (1.2f * level));
        } 
        score += points;
        consecutiveEaten++;
        UpdateScore();
        //UpdateCombo();

        // Increase PlayerBody Y scale
        Vector3 newScale = playerBody.transform.localScale;
        newScale.y += 0.1f;
        playerBody.transform.localScale = newScale;

        // check for win condition
        if (playerBody.transform.localScale.y >= maxBodyHeightScale) {
            NextLevel();
        }

        // if (consecutiveEaten >= 3) {
        //     consecutiveEaten = 0;
        //     ResetCombo();
        //     boundaryDestroyer.DecrementAngerMeter(0.1f); // Decrease anger meter by 10%
        // }
    }

    private void NextLevel() {
        level++;

        // reset body size to default
        Vector3 newScale = playerBody.transform.localScale;
        newScale.y = minBodyHeightScale;
        playerBody.transform.localScale = newScale;

        // reset anger level
        boundaryDestroyer.ResetAngerMeter();
        // increase speed of the background music
        SoundManager.instance.SetBackgroundMusicPitch(1.05f);

        // increase food speed and points
        foodSpawner.IncreaseFoodSpeed(1.2f);
        pointMultiplier *= 1.2f;

        UpdateLevelText();
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