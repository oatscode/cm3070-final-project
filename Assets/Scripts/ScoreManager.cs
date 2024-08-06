using UnityEngine;
using System.Collections;
using TMPro;

public class ScoreManager : MonoBehaviour {
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI multText;
    public TextMeshProUGUI scoreFlashText;
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
        UpdateMultiplierText();
    }

    private void UpdateLevelText() {
        levelText.text = "Level: " + level;
    }

    private void UpdateMultiplierText() {
        if (multText != null) {
            multText.text = pointMultiplier.ToString("F1") + "x";
        }
    }

    private IEnumerator ShowScoreFlash(int points) {
        if (scoreFlashText != null) {
            scoreFlashText.text = "+" + points.ToString();
            yield return new WaitForSeconds(0.3f);
            scoreFlashText.text = "";
        }
    }

    public void AddScore(int basePoints) {
        //float levelMultiplier = 1f + (level - 1) * 0.1f;
        int points = Mathf.RoundToInt(basePoints * pointMultiplier);
        score += points;
        consecutiveEaten++;
        UpdateScore();
        //UpdateCombo();

        StartCoroutine(ShowScoreFlash(points));

        // Increase PlayerBody Y scale
        Vector3 newScale = playerBody.transform.localScale;
        newScale.y += 0.2f;
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
        foodSpawner.IncreaseFoodSpeed(1.3f);
        pointMultiplier += 0.5f;
        UpdateMultiplierText();

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