using UnityEngine;
using System.Collections;
using TMPro;

public class ScoreManager : MonoBehaviour {
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI multText;
    public TextMeshProUGUI scoreFlashText;
    public GameObject levelFlashText;
    public GameObject uiCanvas;

    private int score = 0;
    public int level = 1;
    private const float minBodyHeightScale = 1f;
    private const float maxBodyHeightScale = 10f;
    private const float foodSpeedMultiplier = 1.3f;
    private const float audioPitchMultiplier = 0.05f;
    private const float pointsFlashDuration = 0.3f;
    private const float pointsFlashXPos = -4.75f;
    private const float playerSizeIncrement = 0.5f;
    private const float levelUpAnimationDuration = 1.3f;
    private const float maxAudioPitch = 1f;

    public BoundaryDestroyer boundaryDestroyer;
    public GameObject playerBody;
    public PlayerController playerController;
    public FoodSpawner foodSpawner;

    private float pointMultiplier = 1f;
    private const float pointMultiplierIncrement = 0.5f;

    private void Start() {
        UpdateScore();
        UpdateLevelText();
        UpdateMultiplierText();
        levelFlashText.SetActive(false);
    }

    private void UpdateScore() {
        scoreText.text = score.ToString();
    }

    private void UpdateLevelText() {
        levelText.text = level.ToString();
    }

    private void UpdateMultiplierText() {
        multText.text = pointMultiplier.ToString("F1");
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
            scoreFlashRect.position = new Vector3(pointsFlashXPos, playerPos.y, transform.position.z);

            scoreFlashText.text = "+" + points.ToString();
            yield return new WaitForSeconds(pointsFlashDuration);
            scoreFlashText.text = null;
        }
    }

    private void UpdatePlayerBodySize() {
        Vector3 newScale = playerBody.transform.localScale;
        newScale.y += playerSizeIncrement;
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
        float newPitch = maxAudioPitch + (level - 1) * audioPitchMultiplier;
        SoundManager.instance.PlayLevelUp();
        SoundManager.instance.SetBackgroundMusicPitch(newPitch);
        foodSpawner.IncreaseFoodSpeed(foodSpeedMultiplier);
        pointMultiplier += pointMultiplierIncrement;
        UpdateMultiplierText();
    }

    private IEnumerator PlayLevelUpAnimation() {
        levelFlashText.SetActive(true);
        yield return new WaitForSeconds(levelUpAnimationDuration);
        levelFlashText.SetActive(false);
    }
}
