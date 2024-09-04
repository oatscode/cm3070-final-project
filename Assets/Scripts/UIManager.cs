using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour {
    // public variables adjustable in Unity
    public TextMeshProUGUI scoreText; // UI text to display the current score
    public TextMeshProUGUI levelText; // UI text to display the current level
    public TextMeshProUGUI multText; // UI text to display the current score multiplier
    public TextMeshProUGUI pointsFlashText; // UI text to flash points gained by eating a food
    public GameObject levelFlashText; // UI text to flash when player levels up
    public GameObject uiCanvas; // canvas containing the core UI elements

    public AngerManager angerManager; // to access score data
    public GameObject playerBody; 
    public GameController gameController; // to access player data
    public FoodSpawner foodSpawner; // to access food spawner data

    // variables for internal logic
    private int score = 0; // current score
    public int level = 1; // current level
    private float pointMultiplier = 1f; // current multiplier for points scored

    // constants for game mechanics
    private const float minBodyHeightScale = 1f; // minimum player body size
    private const float maxBodyHeightScale = 10f; // maximum player body size before leveling up
    private const float foodSpeedMultiplier = 1.3f; // multiplier to increase food speed at level up
    private const float audioPitchMultiplier = 0.05f; // multiplier to increase background music pitch at level up
    private const float pointsFlashDuration = 0.3f; // duration for points flash text is visible
    private const float pointsFlashXPos = -4.75f; // X position for the points flash text
    private const float playerSizeIncrement = 0.5f; // player body height increment amount per score addition
    private const float levelUpAnimationDuration = 1.3f; // duration of the level up flash animation
    private const float maxAudioPitch = 1f; // maximum pitch for background music
    private const float pointMultiplierIncrement = 0.5f; // increment in the points per food multiplier at level up

    private void Start() {
        // initialize UI elements
        UpdateScore();
        UpdateLevelText();
        UpdateMultiplierText();
        levelFlashText.SetActive(false); // ensure level up flash text is hidden at the start
    }

    // updates the score UI text with the current score
    private void UpdateScore() {
        scoreText.text = score.ToString();
    }

    // updates the level UI text with the current level
    private void UpdateLevelText() {
        levelText.text = level.ToString();
    }

    // updates the multiplier UI text with the current points multiplier
    private void UpdateMultiplierText() {
        multText.text = pointMultiplier.ToString("F1");
    }

    // returns the current score
    public int GetCurrentScore() {
        return score;
    }

    // adds points to the score, updates the player body size, and checks for level up
    public void AddScore(int basePoints) {
        int points = Mathf.RoundToInt(basePoints * pointMultiplier);
        score += points;
        UpdateScore();
        StartCoroutine(ShowPointsFlash(points)); // flash the points added to the score
        UpdatePlayerBodySize(); // increase the player body size

        // level up if the player body has reached the max size
        if (playerBody.transform.localScale.y >= maxBodyHeightScale) {
            LevelUp();
        }
    }

    // display the points added to the score near the player
    private IEnumerator ShowPointsFlash(int points) {
        if (pointsFlashText != null) {
            RectTransform scoreFlashRect = pointsFlashText.GetComponent<RectTransform>();
            Vector3 playerPos = playerBody.transform.position;
            scoreFlashRect.position = new Vector3(pointsFlashXPos, playerPos.y, transform.position.z);

            pointsFlashText.text = "+" + points.ToString(); // display the points gained
            yield return new WaitForSeconds(pointsFlashDuration);
            pointsFlashText.text = null; // clear the flash text
        }
    }

    // increases the player body size as they eat food
    private void UpdatePlayerBodySize() {
        Vector3 newScale = playerBody.transform.localScale;
        newScale.y += playerSizeIncrement; // increment the height of the player body
        playerBody.transform.localScale = newScale;
    }

    // level up logic: reset player size, adjusting game difficulty, play animation
    private void LevelUp() {
        level++;
        ResetPlayerBodySize();
        angerManager.ResetAngerMeter();
        AdjustGameDifficulty();
        UpdateLevelText();
        StartCoroutine(PlayLevelUpAnimation());
    }

    // resets player body size to the initial minimum scale
    private void ResetPlayerBodySize() {
        Vector3 newScale = playerBody.transform.localScale;
        newScale.y = minBodyHeightScale;
        playerBody.transform.localScale = newScale;
    }

    // adjusts the game difficulty: increase food speed, background music pitch, and point multiplier
    private void AdjustGameDifficulty() {
        float newPitch = maxAudioPitch + (level - 1) * audioPitchMultiplier; // calculate new music pitch
        SoundManager.instance.PlayLevelUp();
        SoundManager.instance.SetBackgroundMusicPitch(newPitch); // set new music pitch
        foodSpawner.IncreaseFoodSpeed(foodSpeedMultiplier); // increase food speeds
        pointMultiplier += pointMultiplierIncrement; // increase points multiplier
        UpdateMultiplierText();
    }

    // display a flash animation on level up
    private IEnumerator PlayLevelUpAnimation() {
        levelFlashText.SetActive(true);
        yield return new WaitForSeconds(levelUpAnimationDuration);
        levelFlashText.SetActive(false);
    }
}
