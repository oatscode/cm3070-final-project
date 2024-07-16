using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5;
    public float vInput;
    public Sprite closedMouthSprite;
    public Sprite openMouthSprite;
    public float openMouthDuration = 0.5f;
    private SpriteRenderer spriteRenderer;
    private Coroutine openMouthCoroutine;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText; 
    private bool isMouthOpen = false;
    private int score = 0;
    private int consecutiveEaten = 0; 
    public BoundaryDestroyer boundaryDestroyer; 
    public GameObject playerBody;
    private float screenHeight;

    // Start is called before the first frame update
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedMouthSprite;
        UpdateScore();
        ResetCombo(); // Initialize combo text

        // Calculate screen height in world units
        screenHeight = Camera.main.orthographicSize * 2;
    }

    // Update is called once per frame
    void Update() {
        float vInput = Input.GetAxisRaw("Vertical");
        transform.Translate(Vector2.up * vInput * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (openMouthCoroutine != null)
            {
                StopCoroutine(openMouthCoroutine);
            }
            openMouthCoroutine = StartCoroutine(OpenMouth());
        }
    }

    IEnumerator OpenMouth() {
        isMouthOpen = true;
        spriteRenderer.sprite = openMouthSprite;
        yield return new WaitForSeconds(openMouthDuration);
        spriteRenderer.sprite = closedMouthSprite;
        isMouthOpen = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Food") && isMouthOpen) {
            Destroy(collision.gameObject);

            score += 100;
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
