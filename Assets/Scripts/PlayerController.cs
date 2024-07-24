using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5;
    public float speedBoostMultiplier = 2f; // Multiplier for speed boost
    public float speedBoostDuration = 10f; // Duration for speed boost
    private SpriteRenderer spriteRenderer;
    private Coroutine openMouthCoroutine;
    private Coroutine speedBoostCoroutine;
    private Coroutine iceCreamCoroutine;
    // private Coroutine magnetCoroutine;
    private bool isMouthOpen = false;
    public Sprite closedMouthSprite;
    public Sprite openMouthSprite;
    public float openMouthDuration = 0.5f;
    public GameObject playerBody;
    // private float screenHeight;
    private float originalMoveSpeed;
    private bool isIceCreamEffectActive = false;

    public TextMeshProUGUI powerText;

    public GameObject boundaryTop;
    public GameObject boundaryBottom;
    private float topBoundary;
    private float bottomBoundary;

    // Track all FoodMover instances
    // private List<FoodMover> activeFoodMovers = new List<FoodMover>();

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedMouthSprite;

        topBoundary = boundaryTop.transform.position.y;
        bottomBoundary = boundaryBottom.transform.position.y;

        // screenHeight = Camera.main.orthographicSize * 2;
        originalMoveSpeed = moveSpeed;

        if (powerText != null) {
            powerText.text = "";
        }

    }

    private void Update() {
        float vInput = Input.GetAxisRaw("Vertical");
        Vector3 newPosition = transform.position + new Vector3(0, vInput * moveSpeed * Time.deltaTime, 0);

        // Clamp the player's position within the top and bottom boundaries
        newPosition.y = Mathf.Clamp(newPosition.y, bottomBoundary, topBoundary);
        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (openMouthCoroutine != null)
            {
                StopCoroutine(openMouthCoroutine);
            }
            openMouthCoroutine = StartCoroutine(OpenMouth());
        }
    }

    private IEnumerator OpenMouth() {
        isMouthOpen = true;
        spriteRenderer.sprite = openMouthSprite;
        yield return new WaitForSeconds(openMouthDuration);
        spriteRenderer.sprite = closedMouthSprite;
        isMouthOpen = false;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.CompareTag("Food") && isMouthOpen) {
            Food food = collision.GetComponent<Food>();
            food.OnEaten(FindObjectOfType<ScoreManager>(), this);
        }
    }

    public void ActivateSpeedBoost() {
        if (speedBoostCoroutine != null) {
            StopCoroutine(speedBoostCoroutine);
        }
        speedBoostCoroutine = StartCoroutine(SpeedBoost());
    }

    private IEnumerator SpeedBoost() {
        moveSpeed *= speedBoostMultiplier;

        if (powerText != null) {
            powerText.text = "<color=red>SPEED</color>";
        }

        yield return new WaitForSeconds(speedBoostDuration);

        moveSpeed = originalMoveSpeed;

        // Reset PowerText
        if (powerText != null) {
            powerText.text = "";
        }
    }

    public void ActivateIceCreamEffect() {
        if (iceCreamCoroutine != null) {
            StopCoroutine(iceCreamCoroutine);
        }
        iceCreamCoroutine = StartCoroutine(IceCreamEffect());
    }

    private IEnumerator IceCreamEffect() {
        isIceCreamEffectActive = true;

        // Get all current FoodMover instances
        FoodMover[] foodMovers = FindObjectsOfType<FoodMover>();

        // Reduce speed by half
        foreach (var foodMover in foodMovers) {
            foodMover.SetSpeed(foodMover.GetSpeed() / 2);
        }

        // Update PowerText to show "SLOW" in blue
        if (powerText != null) {
            powerText.text = "<color=blue>SLOW</color>";
        }

        // Slow down background music
        SoundManager.instance.SetBackgroundMusicPitch(0.5f);

        // Double the spawn intervals for all foods
        //foodSpawner.AdjustSpawnIntervals(2f);

        yield return new WaitForSeconds(10f); // Duration of the Ice Cream effect

        // Reset speed to original for all current food instances
        foreach (var foodMover in foodMovers) {
            foodMover.ResetToOriginalSpeed();
        }

        isIceCreamEffectActive = false;

        // Reset spawn intervals to original
        //foodSpawner.AdjustSpawnIntervals(0.5f);

        // Reset background music pitch
        SoundManager.instance.SetBackgroundMusicPitch(1f);

        // Reset PowerText
        if (powerText != null) {
            powerText.text = "";
        }
    }

    public bool IsIceCreamEffectActive() {
        return isIceCreamEffectActive;
    }

    // public void ActivateMagnetEffect() {
    //     if (magnetCoroutine != null) {
    //         StopCoroutine(magnetCoroutine);
    //     }
    //     magnetCoroutine = StartCoroutine(MagnetEffect());
    // }

    // private IEnumerator MagnetEffect() {
    //     // Placeholder for Magnet effect
    //     Debug.Log("Magnet effect activated!");
    //     yield return new WaitForSeconds(10f); // Example duration
    //     Debug.Log("Magnet effect ended.");
    // }
}