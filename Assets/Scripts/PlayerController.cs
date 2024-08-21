using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    public GameObject playerBody;
    private SpriteRenderer playerHeadSpriteRenderer;
    public float moveSpeed = 5;
    private float originalMoveSpeed;
    public float speedBoostMultiplier = 2f;
    public float speedBoostDuration = 10f; 
    public float magnetDuration = 10f; 

    // mouth open/close variables
    private bool isMouthOpen = false;
    public float openMouthDuration = 0.25f;
    private Coroutine openMouthCoroutine;
    public Sprite closedMouthSprite;
    public Sprite openMouthSprite;
    
    private Coroutine speedPowerCoroutine;
    private Coroutine slowPowerCoroutine;
    private Coroutine magnetPowerCoroutine;
    private Coroutine rottenPowerCoroutine;
    
    private bool isSlowPowerActive = false;
    private bool isMagnetPowerActive = false;

    public TextMeshProUGUI powerText;

    public GameObject boundaryTop;
    public GameObject boundaryBottom;
    private float boundaryTopYPos;
    private float boundaryBottomYPos;

    // power-up variables
    public Image[] powerUpSlots;
    private bool isSpeedPowerReady = false;
    private bool isSlowPowerReady = false;
    private bool isMagnetPowerReady = false;
    public Sprite speedPowerSprite;
    public Sprite slowPowerSprite;
    public Sprite magnetPowerSprite;

    // the sprites for the different monster expressions
    public Sprite[] mouthClosedSprites;
    public Sprite[] mouthOpenSprites;

    // list to track FoodMovers affected by magnet effect
    private List<FoodMover> magnetAffectedFoodMovers = new List<FoodMover>();

    private BoundaryDestroyer boundaryDestroyer;
    private ScoreManager scoreManager;

    private void Start() {
        boundaryDestroyer = FindObjectOfType<BoundaryDestroyer>();
        scoreManager = FindObjectOfType<ScoreManager>();
        playerHeadSpriteRenderer = GetComponent<SpriteRenderer>();
        playerHeadSpriteRenderer.sprite = closedMouthSprite;

        boundaryTopYPos = boundaryTop.transform.position.y;
        boundaryBottomYPos = boundaryBottom.transform.position.y;

        originalMoveSpeed = moveSpeed;

        powerText.text = null;
    }

    private void Update() {
        HandleMovement();
        HandleInput();
    }

    private void HandleMovement() {
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 newPosition = transform.position + new Vector3(0, verticalInput * moveSpeed * Time.deltaTime, 0);
        newPosition.y = Mathf.Clamp(newPosition.y, boundaryBottomYPos, boundaryTopYPos);
        transform.position = newPosition;
    }

    private void HandleInput() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (openMouthCoroutine != null) StopCoroutine(openMouthCoroutine);
            openMouthCoroutine = StartCoroutine(OpenMouth());
        }

                if (Input.GetKeyDown(KeyCode.Space)) {
            if (openMouthCoroutine != null) {
                StopCoroutine(openMouthCoroutine);
            }
            openMouthCoroutine = StartCoroutine(OpenMouth());
        }

        // check for activation of stored power-ups
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) {
            ActivateSpeedPower();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) {
            ActivateSlowPower();
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) {
            ActivateMagnetPower();
        }
    }

    public void ActivateRottenPower() {
        if (rottenPowerCoroutine != null) {
            StopCoroutine(rottenPowerCoroutine);
        }
        rottenPowerCoroutine = StartCoroutine(RottenEffect());
    }

    private IEnumerator RottenEffect() {
        float elapsed = 0f;
        Vector3 originalPosition = transform.position;

        SoundManager.instance.PlaySick();

        while (elapsed < 1f) {
            float jitterAmount = Mathf.Sin(elapsed * 40) * 0.1f;
            transform.position = new Vector3(originalPosition.x, originalPosition.y + jitterAmount, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition; 
    }

    private IEnumerator OpenMouth() {
        isMouthOpen = true;
        int angerIndex = Mathf.Clamp(Mathf.FloorToInt(boundaryDestroyer.angerMeterValue * 10), 0, 9);
        playerHeadSpriteRenderer.sprite = mouthOpenSprites[angerIndex];
        yield return new WaitForSeconds(openMouthDuration);
        // playerHeadSpriteRenderer.sprite = closedMouthSprite;
        
        playerHeadSpriteRenderer.sprite = mouthClosedSprites[angerIndex];
        isMouthOpen = false;
    }

    public void StorePowerUp(Food.PowerUpType powerUpType) {
        switch (powerUpType) {
            case Food.PowerUpType.Speed:
                isSpeedPowerReady = true;
                UpdatePowerUpUI(0, speedPowerSprite);
                break;
            case Food.PowerUpType.Slow:
                isSlowPowerReady = true;
                UpdatePowerUpUI(1, slowPowerSprite);
                break;
            case Food.PowerUpType.Magnet:
                isMagnetPowerReady = true;
                UpdatePowerUpUI(2, magnetPowerSprite);
                break;
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

    private void ActivateSpeedPower() {
        if (isSpeedPowerReady) {
            ActivateSpeedBoost();
            isSpeedPowerReady = false;
            UpdatePowerUpUI(0, null);
            SoundManager.instance.PlaySpeed();
        }
    }

    private void ActivateSlowPower() {
        if (isSlowPowerReady) {
            ActivateIceCreamEffect();
            isSlowPowerReady = false;
            UpdatePowerUpUI(1, null);
        }
    }

    private void ActivateMagnetPower() {
        if (isMagnetPowerReady) {
            ActivateMagnetEffect();
            isMagnetPowerReady = false;
            UpdatePowerUpUI(2, null);
            SoundManager.instance.PlayMagnet();
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        // "eat" a food if it's colliding with the player and player mouth is open
        if (collision.CompareTag("Food") && isMouthOpen) {
            Food food = collision.GetComponent<Food>();
            food.OnEaten(scoreManager, this);
          }
    }

    public void ActivateSpeedBoost() {
        if (speedPowerCoroutine != null) {
            StopCoroutine(speedPowerCoroutine);
        }
        speedPowerCoroutine = StartCoroutine(SpeedBoost());
    }

    private IEnumerator SpeedBoost() {
        moveSpeed *= speedBoostMultiplier;

        if (powerText != null) {
            powerText.text = "<color=red>SPEED</color>";
        }

        yield return new WaitForSeconds(speedBoostDuration);

        moveSpeed = originalMoveSpeed;

        // reset PowerText
        if (powerText != null) {
            powerText.text = "";
        }
    }

    public void ActivateIceCreamEffect() {
        if (slowPowerCoroutine != null) {
            StopCoroutine(slowPowerCoroutine);
        }
        slowPowerCoroutine = StartCoroutine(IceCreamEffect());
    }

    private IEnumerator IceCreamEffect() {
        isSlowPowerActive = true;

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

        isSlowPowerActive = false;

        // reset spawn intervals to original
        // foodSpawner.AdjustSpawnIntervals(0.5f);

        // calculate the BGM pitch based on the level
        float currentLevelPitch = 1f + (scoreManager.level - 1) * 0.05f;
        SoundManager.instance.SetBackgroundMusicPitch(currentLevelPitch);

        powerText.text = "";
    }

    public bool IsIceCreamEffectActive() {
        return isSlowPowerActive;
    }

    public bool IsMagnetEffectActive() {
        return isMagnetPowerActive;
    }

    public void ActivateMagnetEffect() {
        if (magnetPowerCoroutine != null) {
            StopCoroutine(magnetPowerCoroutine);
        }
        magnetPowerCoroutine = StartCoroutine(MagnetEffect());
    }

    private IEnumerator MagnetEffect() {
        isMagnetPowerActive = true;

        // get all current FoodMover instances
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

        SoundManager.instance.magnetSound.Stop();
        isMagnetPowerActive = false;

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

    public void ResetPlayerBodySize() {
        playerBody.transform.localScale = new Vector3(1f, 0.66f, playerBody.transform.localScale.z);
        playerBody.transform.localPosition = new Vector3(0, playerBody.transform.localPosition.y, playerBody.transform.localPosition.z);
    }
}