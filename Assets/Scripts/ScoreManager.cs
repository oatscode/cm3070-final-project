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
    public GameObject levelFlashText;
    public GameObject uiCanvas;

    private int score = 0;
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
        UpdateLevelText();
        UpdateMultiplierText();
        levelFlashText.SetActive(false);
    }

    private void UpdateScore() {
        scoreText.text = "SCORE " + score;
    }

    private void UpdateLevelText() {
        levelText.text = "LEVEL " + level;
    }

    private void UpdateMultiplierText() {
        if (multText != null) {
            multText.text = pointMultiplier.ToString("F1") + "X";
        }
    }

    public int GetCurrentScore() {
        return score;
    }

    public void AddScore(int basePoints) {
        int points = Mathf.RoundToInt(basePoints * pointMultiplier);
        score += points;
        UpdateScore();
        StartCoroutine(ShowScoreFlash(points));
        UpdatePlayerBodySize();

        if (playerBody.transform.localScale.y >= maxBodyHeightScale) {
            LevelUp();
        }
    }

    private IEnumerator ShowScoreFlash(int points) {
        if (scoreFlashText != null) {
            RectTransform scoreFlashRect = scoreFlashText.GetComponent<RectTransform>();
            Vector3 playerPos = playerBody.transform.position;
            scoreFlashRect.position = new Vector3(-5, playerPos.y, transform.position.z);

            scoreFlashText.text = "+" + points.ToString();
            yield return new WaitForSeconds(0.3f);
            scoreFlashText.text = null;
        }
    }

    private void UpdatePlayerBodySize() {
        Vector3 newScale = playerBody.transform.localScale;
        newScale.y += 0.2f;
        playerBody.transform.localScale = newScale;
    }

    private void LevelUp() {
        level++;
        ResetPlayerBodySize();
        boundaryDestroyer.ResetAngerMeter();
        AdjustGameDifficulty();
        UpdateLevelText();
        StartCoroutine(PlayLevelUpAnimation());
    }

    private void ResetPlayerBodySize() {
        Vector3 newScale = playerBody.transform.localScale;
        newScale.y = minBodyHeightScale;
        playerBody.transform.localScale = newScale;
    }

    private void AdjustGameDifficulty() {
        float newPitch = 1f + (level - 1) * 0.05f;
        SoundManager.instance.PlayLevelUp();
        SoundManager.instance.SetBackgroundMusicPitch(newPitch);
        foodSpawner.IncreaseFoodSpeed(1.3f);
        pointMultiplier += 0.5f;
        UpdateMultiplierText();
    }

    private IEnumerator PlayLevelUpAnimation() {
        levelFlashText.SetActive(true);
        yield return new WaitForSeconds(1.3f);
        levelFlashText.SetActive(false);
    }
}
