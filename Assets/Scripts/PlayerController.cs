using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    public GameObject playerBody;
    private SpriteRenderer spriteRenderer;
    public float moveSpeed = 5;  // player move speed
    public float speedBoostMultiplier = 2f; // Multiplier for speed boost
    public float speedBoostDuration = 10f; // Duration for speed boost
    public float magnetDuration = 10f; // Duration for magnet effect

    // mouth open/close variables
    private bool isMouthOpen = false;
    public float openMouthDuration = 0.25f;
    private Coroutine openMouthCoroutine;
    public Sprite closedMouthSprite;
    public Sprite openMouthSprite;
    
    
    private Coroutine speedBoostCoroutine;
    private Coroutine iceCreamCoroutine;
    private Coroutine magnetCoroutine;
    
    private float originalMoveSpeed;
    private bool isIceCreamEffectActive = false;
    private bool isMagnetEffectActive = false;

    public TextMeshProUGUI powerText;

    public GameObject boundaryTop;
    public GameObject boundaryBottom;
    private float topBoundary;
    private float bottomBoundary;

    // Power-up variables
    public Image[] powerUpSlots;
    private bool hasChilliPower = false;
    private bool hasIceCreamPower = false;
    private bool hasMagnetPower = false;
    public Sprite chilliSprite;
    public Sprite iceCreamSprite;
    public Sprite magnetSprite;

    // List to track FoodMovers affected by magnet effect
    private List<FoodMover> magnetAffectedFoodMovers = new List<FoodMover>();

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedMouthSprite;

        topBoundary = boundaryTop.transform.position.y;
        bottomBoundary = boundaryBottom.transform.position.y;

        originalMoveSpeed = moveSpeed;

        powerText.text = null;
    }

    private void Update() {
        float vInput = Input.GetAxisRaw("Vertical");
        Vector3 newPosition = transform.position + new Vector3(0, vInput * moveSpeed * Time.deltaTime, 0);

        // keep the player within the top and bottom boundaries
        newPosition.y = Mathf.Clamp(newPosition.y, bottomBoundary, topBoundary);
        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (openMouthCoroutine != null) {
                StopCoroutine(openMouthCoroutine);
            }
            openMouthCoroutine = StartCoroutine(OpenMouth());
        }

       // Check for activation of stored power-ups
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) {
            ActivateStoredChilliPowerUp();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) {
            ActivateStoredIceCreamPowerUp();
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) {
            ActivateStoredMagnetPowerUp();
        }
    }

    private IEnumerator OpenMouth() {
        isMouthOpen = true;
        spriteRenderer.sprite = openMouthSprite;
        yield return new WaitForSeconds(openMouthDuration);
        spriteRenderer.sprite = closedMouthSprite;
        isMouthOpen = false;
    }

    public void StorePowerUp(Food.PowerUpType powerUpType) {
        if (powerUpType == Food.PowerUpType.Chilli) {
            hasChilliPower = true;
            UpdatePowerUpUI(0, chilliSprite);
        } else if (powerUpType == Food.PowerUpType.IceCream) {
            hasIceCreamPower = true;
            UpdatePowerUpUI(1, iceCreamSprite);
        } else if (powerUpType == Food.PowerUpType.Magnet) {
            hasMagnetPower = true;
            UpdatePowerUpUI(2, magnetSprite);
        }
    }

    private void UpdatePowerUpUI(int slotIndex, Sprite sprite) {
        if (slotIndex < powerUpSlots.Length) {
            powerUpSlots[slotIndex].sprite = sprite;
            powerUpSlots[slotIndex].preserveAspect = true;
            Color colour = powerUpSlots[slotIndex].color;
            colour.a = sprite ? 1f : 0.125f; // set alpha to 255 (1f) if sprite is not null, otherwise to 32 (0.125f)
            powerUpSlots[slotIndex].color = colour;
        }
    }

    private void ActivateStoredChilliPowerUp() {
        if (hasChilliPower) {
            ActivateSpeedBoost();
            hasChilliPower = false;
            UpdatePowerUpUI(0, null);
        }
    }

    private void ActivateStoredIceCreamPowerUp() {
        if (hasIceCreamPower) {
            ActivateIceCreamEffect();
            hasIceCreamPower = false;
            UpdatePowerUpUI(1, null);
        }
    }

    private void ActivateStoredMagnetPowerUp() {
        if (hasMagnetPower) {
            ActivateMagnetEffect();
            hasMagnetPower = false;
            UpdatePowerUpUI(2, null);
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        // "eat" a food if it's colliding with the player and player mouth is open
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

        // get all current FoodMover instances
        FoodMover[] foodMovers = FindObjectsOfType<FoodMover>();

        // reduce speed by half
        foreach (var foodMover in foodMovers) {
            foodMover.SetSpeed(foodMover.GetSpeed() / 2);
        }

        // update power text box to show "SLOW" in blue
        if (powerText != null) {
            powerText.text = "<color=blue>SLOW</color>";
        }

        // slow down background music
        SoundManager.instance.SetBackgroundMusicPitch(0.5f);

        // double the spawn intervals for all foods
        // foodSpawner.AdjustSpawnIntervals(2f);

        yield return new WaitForSeconds(10f); 

        // reset speed to original for all current food instances
        foreach (var foodMover in foodMovers) {
            foodMover.ResetToOriginalSpeed();
        }

        isIceCreamEffectActive = false;

        // reset spawn intervals to original
        // foodSpawner.AdjustSpawnIntervals(0.5f);

        // reset background music pitch
        SoundManager.instance.SetBackgroundMusicPitch(1f);

        // reset PowerText
        if (powerText != null) {
            powerText.text = "";
        }
    }

    public bool IsIceCreamEffectActive() {
        return isIceCreamEffectActive;
    }

    public bool IsMagnetEffectActive() {
        return isMagnetEffectActive;
    }

    public void ActivateMagnetEffect() {
        if (magnetCoroutine != null) {
            StopCoroutine(magnetCoroutine);
        }
        magnetCoroutine = StartCoroutine(MagnetEffect());
    }

    private IEnumerator MagnetEffect() {
        isMagnetEffectActive = true;

        // Get all current FoodMover instances
        FoodMover[] foodMovers = FindObjectsOfType<FoodMover>();

        // change movement pattern to move towards the player
        foreach (var foodMover in foodMovers) {
            foodMover.SetMagnetTarget(transform);
            magnetAffectedFoodMovers.Add(foodMover);
        }

        if (powerText != null) {
            powerText.text = "<color=yellow>MAGNET</color>";
        }

        yield return new WaitForSeconds(magnetDuration);

        isMagnetEffectActive = false;

        // reset movement pattern to original
        foreach (var foodMover in magnetAffectedFoodMovers) {
            foodMover.ResetToOriginalMovementPattern();
        }

        magnetAffectedFoodMovers.Clear();

        // reset PowerText
        if (powerText != null) {
            powerText.text = "";
        }
    }
}