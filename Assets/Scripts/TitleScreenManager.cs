using UnityEngine;
using UnityEngine.SceneManagement;  
using System.Collections;
using TMPro;  

public class TitleScreenManager : MonoBehaviour {
    public GameObject titleScreenCanvas;  
    public TextMeshProUGUI pressSpaceText;  
    public float blinkSpeed = 0.5f;  
    private bool gameStarted = false;
    private const KeyCode startGameKey = KeyCode.Space;

    private void Start() {
        titleScreenCanvas.SetActive(true);
        StartCoroutine(BlinkPressSpaceText());
    }

    private void Update() {
        if (!gameStarted && Input.GetKeyDown(startGameKey)) {
            StartGame();
        }
    }

    private void StartGame() {
        gameStarted = true;
        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator BlinkPressSpaceText() {
        while (!gameStarted) {
            pressSpaceText.alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            yield return null;
        }
    }
}
