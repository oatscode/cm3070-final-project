using UnityEngine;
using UnityEngine.SceneManagement;  
using System.Collections;
using TMPro;  

public class TitleScreenManager : MonoBehaviour
{
    public GameObject titleScreenCanvas;  // Reference to the title screen UI Canvas
    public TextMeshProUGUI pressSpaceText;  // Reference to the "Press SPACE to start" TMP element
    public float blinkSpeed = 0.5f;  // Speed of blinking effect for the text

    private bool gameStarted = false;

    private void Start() {
        titleScreenCanvas.SetActive(true);  // Ensure the title screen is active
        StartCoroutine(BlinkPressSpaceText());  // Start the blinking text effect
    }

    private void Update() {
        if (!gameStarted && Input.GetKeyDown(KeyCode.Space)) {
            StartGame();  // Start the game when SPACE is pressed
        }
    }

    private void StartGame() {
        gameStarted = true;
        //titleScreenCanvas.SetActive(false);  // Hide the title screen UI
        SceneManager.LoadScene("MainScene");
        
        // You can either load a different scene here or activate gameplay elements in the same scene.
        // For example, if you want to load a different scene:
        // SceneManager.LoadScene("MainGameScene");

        // Or you can activate gameplay elements here, such as starting the player, enabling the game objects, etc.
    }

    private IEnumerator BlinkPressSpaceText() {
        while (!gameStarted) {
            pressSpaceText.alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            yield return null;
        }
    }
}
