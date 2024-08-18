using UnityEngine;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class ScoreManager : MonoBehaviour {
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI multText;
    public TextMeshProUGUI scoreFlashText;
    public GameObject levelFlashText;
    public GameObject uiCanvas;
    private int score = 0;
    private int consecutiveEaten = 0;
    private float minBodyHeightScale = 0.66f;
    private float maxBodyHeightScale = 6.8f;
    public int level = 1; 
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
        levelFlashText.SetActive(false);
    }

    private void UpdateLevelText() {
        levelText.text = "Level: " + level;
    }

    public int GetCurrentScore() {
        return score;
    }

    private void UpdateMultiplierText() {
        if (multText != null) {
            multText.text = pointMultiplier.ToString("F1") + "x";
        }
    }

    private IEnumerator PlayLevelUpAnimation() {
        // Enable the LevelFlashText object
        levelFlashText.SetActive(true);

        // Wait for the duration of the animation (assuming 1 second for the example)
        yield return new WaitForSeconds(1.3f); // Adjust this to match your animation length

        // Disable the LevelFlashText object after the animation is complete
        levelFlashText.SetActive(false);
    }

    private IEnumerator ShowScoreFlash(int points) {
        if (scoreFlashText != null) {
            int scoreFlashXpos = -5;
            RectTransform scoreFlashRect = scoreFlashText.GetComponent<RectTransform>();
            Vector3 playerPos = playerBody.transform.position;
            scoreFlashRect.position = new Vector3(scoreFlashXpos, playerPos.y, transform.position.z);

            scoreFlashText.text = "+" + points.ToString();
            yield return new WaitForSeconds(0.3f);
            scoreFlashText.text = null;
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

        // calculate the BGM pitch based on the level
        float newPitch = 1f + (level - 1) * 0.05f;
        SoundManager.instance.SetBackgroundMusicPitch(newPitch);

        // increase food speed and points
        foodSpawner.IncreaseFoodSpeed(1.3f);
        pointMultiplier += 0.5f;
        UpdateMultiplierText();

        UpdateLevelText();

        StartCoroutine(PlayLevelUpAnimation());
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