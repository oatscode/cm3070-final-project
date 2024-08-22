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
    public float powerDuration = 5f; 

    // mouth open/close variables
    private bool isMouthOpen = false;
    public float openMouthDuration = 0.25f;

    
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
            StartCoroutine(OpenMouth());
        }

        // check for activation of stored power-ups
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && isSpeedPowerReady) {
            StartCoroutine(SpeedEffect());
        }

        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) && isSlowPowerReady) {
            StartCoroutine(SlowEffect());
        }

        if ((Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) && isMagnetPowerReady) {
            StartCoroutine(MagnetEffect());
        }
    }

    private IEnumerator OpenMouth() {
        isMouthOpen = true;
        int angerIndex = Mathf.Clamp(Mathf.FloorToInt(boundaryDestroyer.angerMeterValue * 10), 0, 9);
        playerHeadSpriteRenderer.sprite = mouthOpenSprites[angerIndex];
        yield return new WaitForSeconds(openMouthDuration);
        
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

    private void OnTriggerStay2D(Collider2D collision) {
        // "eat" a food if it's colliding with the player and player mouth is open
        if (collision.CompareTag("Food") && isMouthOpen) {
            Food food = collision.GetComponent<Food>();
            food.OnEaten(scoreManager, this);
          }
    }

    public void ResetPlayerBodySize() {
        playerBody.transform.localScale = new Vector3(1f, 0.66f, playerBody.transform.localScale.z);
        playerBody.transform.localPosition = new Vector3(0, playerBody.transform.localPosition.y, playerBody.transform.localPosition.z);
    }

    private IEnumerator SpeedEffect() {
        isSpeedPowerReady = false;
        UpdatePowerUpUI(0, null);
        SoundManager.instance.PlaySpeed();
        moveSpeed *= speedBoostMultiplier;
        powerText.text = "<color=red>SPEED</color>";
        yield return new WaitForSeconds(powerDuration);
        moveSpeed = originalMoveSpeed;
        powerText.text = "";
    }

    public bool IsSlowEffectActive() {
        return isSlowPowerActive;
    }

    private IEnumerator SlowEffect() {
        isSlowPowerReady = false;
        UpdatePowerUpUI(1, null);
        isSlowPowerActive = true;

        // get all current FoodMover instances
        FoodMover[] foodMovers = FindObjectsOfType<FoodMover>();

        // reduce speed by half
        foreach (var foodMover in foodMovers) {
            foodMover.SetSpeed(foodMover.GetSpeed() / 2);
        }

        // update power text box to show "SLOW" in blue
        powerText.text = "<color=blue>SLOW</color>";
        
        // slow down background music
        SoundManager.instance.SetBackgroundMusicPitch(0.5f);

        yield return new WaitForSeconds(powerDuration); 

        // reset speed to original for all current food instances
        foreach (var foodMover in foodMovers) {
            foodMover.ResetToOriginalSpeed();
        }

        isSlowPowerActive = false;

        // calculate the BGM pitch based on the level
        float currentLevelPitch = 1f + (scoreManager.level - 1) * 0.05f;
        SoundManager.instance.SetBackgroundMusicPitch(currentLevelPitch);

        powerText.text = "";
    }

    public bool IsMagnetEffectActive() {
        return isMagnetPowerActive;
    }

    private IEnumerator MagnetEffect() {
        isMagnetPowerReady = false;
        UpdatePowerUpUI(2, null);
        SoundManager.instance.PlayMagnet();
        isMagnetPowerActive = true;

        // get all current FoodMover instances
        FoodMover[] foodMovers = FindObjectsOfType<FoodMover>();

        // change movement pattern to move towards the player
        foreach (var foodMover in foodMovers) {
            foodMover.SetMagnetTarget(transform);
            magnetAffectedFoodMovers.Add(foodMover);
        }
        
        powerText.text = "<color=yellow>MAGNET</color>";
        
        yield return new WaitForSeconds(powerDuration);

        SoundManager.instance.magnetSound.Stop();
        isMagnetPowerActive = false;

        // reset movement pattern to original
        foreach (var foodMover in magnetAffectedFoodMovers) {
            foodMover.ResetToOriginalMovementPattern();
        }

        magnetAffectedFoodMovers.Clear();

        powerText.text = ""; // reset PowerText
        
    }

    public void ActivateRottenPower() {
        StartCoroutine(RottenEffect());
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
}