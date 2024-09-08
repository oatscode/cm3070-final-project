using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
    // public variables adjustable in Unity
    public GameObject playerBody; 
    public GameObject movementGuide; // UI for player movement controls
    public GameObject eatGuide; // UI for eating controls
    public GameObject boundaryTop; // top boundary to restrict player movement
    public GameObject boundaryBottom; // bottom boundary to restrict player movement
    public float moveSpeed = 5; // default player movement speed
    public float speedBoostMultiplier = 2f; // move speed multiplier when speed power-up is active
    public float powerDuration = 5f; // duration (in seconds) of power-up effects 
    public float openMouthDuration = 0.25f; // duration (in seconds) of player mouth open when eat button is pressed
    public TextMeshProUGUI powerText; // UI label to display active power-up
    public Image[] powerUpSlots; // array of slots for displaying power-ups
    private AngerManager angerManager; // to access boundary data
    private UIManager uiManager; // to access score data
    
    // sprites for power-ups
    public Sprite speedPowerSprite;
    public Sprite slowPowerSprite;
    public Sprite magnetPowerSprite;

    // arrays for player sprites for open/closed states based on anger level
    public Sprite[] mouthClosedSprites;
    public Sprite[] mouthOpenSprites;

    // variables for internal logic
    private SpriteRenderer playerHeadSpriteRenderer; // renderer for player's head to change sprites
    private float originalMoveSpeed; // move speed to revert to after power-up effects
    private bool isMouthOpen = false;
    private bool isSlowPowerActive = false;
    private bool isMagnetPowerActive = false;
    private bool isSpeedPowerReady = false;
    private bool isSlowPowerReady = false;
    private bool isMagnetPowerReady = false;
    private float boundaryTopYPos;
    private float boundaryBottomYPos;
    private const int foodSpeedReductionMultiplier = 2;
    private const float slowEffectMusicPitch = 0.5f;
    private const float musicPitchDefault = 1f;
    private const int musicPitchStartLevel = 1;
    private const float musicPitchIncrement = 0.05f;

    // constants for anger meter 
    private const int minAngerIndex = 0;
    private const int maxAngerIndex = 9;
    private const int angerMultiplier = 10;

    // contants for power slot indexes
    private const int speedPowerSlotIndex = 0;
    private const int slowPowerSlotIndex = 1;
    private const int magnetPowerSlotIndex = 2;

    // constants for key bindings
    private const KeyCode speedPowerKey = KeyCode.Y;
    private const KeyCode speedPowerAltKey = KeyCode.Z;
    private const KeyCode slowPowerKey = KeyCode.X;
    private const KeyCode magnetPowerKey = KeyCode.C;
    private const KeyCode eatKey = KeyCode.Space;

    // constants for rotten (jitter) effect
    private const float rottenEffectDuration = 1.5f;
    private const float rottenEffectFrequency = 40f;
    private const float rottenEffectAmplitude = 0.1f;

    // variables for power-up slot state 
    private const float powerSlotDisabledAlpha = 0.125f;
    private const float powerSlotEnabledAlpha = 1f;
    private Color32 defaultPowerSlotColour = new Color32(255,255,255,32);
    private Color32 notReadyPowerSlotColour = new Color32(255,0,0,64);

    // list to track food movers affected by magnet power-up
    private List<FoodMover> magnetAffectedFoodMovers = new List<FoodMover>();

    private void Start() {
        // initialise references and variables
        angerManager = FindObjectOfType<AngerManager>();
        uiManager = FindObjectOfType<UIManager>();
        playerHeadSpriteRenderer = GetComponent<SpriteRenderer>();
         
        // get the Y positions of the top and bottom boundaries
        boundaryTopYPos = boundaryTop.transform.position.y;
        boundaryBottomYPos = boundaryBottom.transform.position.y;

        originalMoveSpeed = moveSpeed;
    }

    private void Update() {
        // handle player movement and input each frame
        HandleMovement();
        HandleInput();
    }

    // handles player vertical movement based on input
    private void HandleMovement() {
        float verticalInput = Input.GetAxisRaw("Vertical");
        // hide the movement guide once the player starts moving
        if (verticalInput != 0 && movementGuide.activeInHierarchy == true) {
            movementGuide.SetActive(false);
        }
        
        // calculate player position based on input and speed, and clamp it within boundaries
        Vector3 newPosition = transform.position + new Vector3(0, verticalInput * moveSpeed * Time.deltaTime, 0);
        newPosition.y = Mathf.Clamp(newPosition.y, boundaryBottomYPos, boundaryTopYPos);
        transform.position = newPosition;
    }

    // handles player input for activating power-ups and opening mouth to eat
    private void HandleInput() {
        if (Input.GetKeyDown(eatKey)) {
            // hide the eat guide when the player opens mouth
            eatGuide.SetActive(false);
            StartCoroutine(OpenMouth());
        }

        // check for power-up activations based on key presses and whether power-ups have been collected
        if (Input.GetKeyDown(speedPowerKey) || Input.GetKeyDown(speedPowerAltKey)) {
            if (isSpeedPowerReady) {
                StartCoroutine(SpeedEffect());
            } else {
                StartCoroutine(PowerNotReady(powerUpSlots[speedPowerSlotIndex]));
            }
        }

        if (Input.GetKeyDown(slowPowerKey)) {
            if (isSlowPowerReady) {
                StartCoroutine(SlowEffect());
            } else {
                StartCoroutine(PowerNotReady(powerUpSlots[slowPowerSlotIndex]));
            }
        }

        if (Input.GetKeyDown(magnetPowerKey)) {
            if (isMagnetPowerReady) {
                StartCoroutine(MagnetEffect());
            } else {
                StartCoroutine(PowerNotReady(powerUpSlots[magnetPowerSlotIndex]));
            }
        }
    }

    // indicate power-up not ready by changing the colour of the respective power-up slot,
    // when the player tries to use a power-up that hasn't been collected
    private IEnumerator PowerNotReady(Image powerUpSlot) {
        powerUpSlot.color = notReadyPowerSlotColour;
        yield return new WaitForSeconds(0.25f);
        powerUpSlot.color = defaultPowerSlotColour;
    }

    // open the player's mouth for a short duration, allowing food to be eaten
    private IEnumerator OpenMouth() {
        isMouthOpen = true;
        // change the player sprite depending on the current anger level
        int angerIndex = Mathf.Clamp(Mathf.FloorToInt(angerManager.angerMeterValue * angerMultiplier), minAngerIndex, maxAngerIndex);
        playerHeadSpriteRenderer.sprite = mouthOpenSprites[angerIndex];
        yield return new WaitForSeconds(openMouthDuration);
        playerHeadSpriteRenderer.sprite = mouthClosedSprites[angerIndex];
        isMouthOpen = false;
    }

    // stores power-ups and updates the UI slot when the player collects a power-up
    public void StorePowerUp(Food.PowerUpType powerUpType) {
        switch (powerUpType) {
            case Food.PowerUpType.Speed:
                isSpeedPowerReady = true;
                UpdatePowerUpUI(speedPowerSlotIndex, speedPowerSprite);
                break;
            case Food.PowerUpType.Slow:
                isSlowPowerReady = true;
                UpdatePowerUpUI(slowPowerSlotIndex, slowPowerSprite);
                break;
            case Food.PowerUpType.Magnet:
                isMagnetPowerReady = true;
                UpdatePowerUpUI(magnetPowerSlotIndex, magnetPowerSprite);
                break;
        }
    }

    // updates respective UI slot with sprite and alpha based on power-up availability
    private void UpdatePowerUpUI(int slotIndex, Sprite sprite) {
        if (slotIndex < powerUpSlots.Length) {
            powerUpSlots[slotIndex].sprite = sprite;
            powerUpSlots[slotIndex].preserveAspect = true;
            Color colour = powerUpSlots[slotIndex].color;
            // set slot alpha to full power-up is available, otherwise set it lower
            if (sprite) {
                colour.a = powerSlotEnabledAlpha;
            } else {
                colour.a = powerSlotDisabledAlpha;
            }
            powerUpSlots[slotIndex].color = colour;
        }
    }

    // detects player collisions with food objects 
    // triggers the food's OnEaten event if player mouth is open
    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.CompareTag("Food") && isMouthOpen) {
            Food food = collision.GetComponent<Food>();
            food.OnEatenTrigger(uiManager, this);
        }
    }

    // activate the speed power-up
    // increase movement speed for a limited time
    private IEnumerator SpeedEffect() {
        isSpeedPowerReady = false;
        UpdatePowerUpUI(speedPowerSlotIndex, null);
        SoundManager.instance.PlaySpeed();
        moveSpeed *= speedBoostMultiplier;
        powerText.text = "<color=red>SPEED</color>";
        yield return new WaitForSeconds(powerDuration);
        moveSpeed = originalMoveSpeed;
        powerText.text = "";
    }

    // returns whether the slow power-up is currently active
    public bool IsSlowEffectActive() {
        return isSlowPowerActive;
    }

    // activate the slow power-up
    // reduce speed of all food movers, and lower the background music pitch
    private IEnumerator SlowEffect() {
        isSlowPowerReady = false;
        UpdatePowerUpUI(slowPowerSlotIndex, null);
        isSlowPowerActive = true;

        // get all current FoodMover instances and reduce their speed
        FoodMover[] foodMovers = FindObjectsOfType<FoodMover>();
        foreach (var foodMover in foodMovers) {
            foodMover.SetSpeed(foodMover.GetSpeed() / foodSpeedReductionMultiplier);
        }

        powerText.text = "<color=blue>SLOW</color>";
        
        // slow down background music
        SoundManager.instance.SetBackgroundMusicPitch(slowEffectMusicPitch);
        yield return new WaitForSeconds(powerDuration); // effect over

        // reset speed to original for all food movers
        foreach (var foodMover in foodMovers) {
            foodMover.ResetToOriginalSpeed();
        }

        isSlowPowerActive = false;

        // reset background music pitch based on the player's current level
        // music pitch increases from level 2 onward
        float currentLevelPitch = musicPitchDefault + 
            (uiManager.level - musicPitchStartLevel) * musicPitchIncrement;
        SoundManager.instance.SetBackgroundMusicPitch(currentLevelPitch);

        powerText.text = null;
    }

    // returns whether the magnet power-up is currently active
    public bool IsMagnetEffectActive() {
        return isMagnetPowerActive;
    }

    // activate the magnet power-up
    // pulling all food towards the player
    private IEnumerator MagnetEffect() {
        isMagnetPowerReady = false;
        UpdatePowerUpUI(magnetPowerSlotIndex, null);
        SoundManager.instance.PlayMagnet();
        isMagnetPowerActive = true;

        // get all current FoodMover instances and set them to move towards the player
        FoodMover[] foodMovers = FindObjectsOfType<FoodMover>();
        foreach (var foodMover in foodMovers) {
            foodMover.SetMagnetTarget(transform);
            magnetAffectedFoodMovers.Add(foodMover);
        }
        
        powerText.text = "<color=yellow>MAGNET</color>";        
        yield return new WaitForSeconds(powerDuration); // effect over
        SoundManager.instance.magnetSound.Stop(); // stop the magnet sound effect
        isMagnetPowerActive = false;

        // reset movement patterns for all food movers affected by the magnet
        foreach (var foodMover in magnetAffectedFoodMovers) {
            foodMover.ResetToOriginalMovementPattern();
        }

        magnetAffectedFoodMovers.Clear();
        powerText.text = null; // clear PowerText
    }

    // activates the rotten (negative) power-up
    public void ActivateRottenPower() {
        StartCoroutine(RottenEffect());
    }

    // apply a short jitter effect when the player eats rotten food
    private IEnumerator RottenEffect() {
        float timeElapsed = 0f;
        Vector3 originalPosition = transform.position;

        SoundManager.instance.PlaySick(); // play sick sound effect

        while (timeElapsed < rottenEffectDuration) {
            float jitterAmount = Mathf.Sin(timeElapsed * rottenEffectFrequency) * rottenEffectAmplitude;
            transform.position = new Vector3(originalPosition.x, originalPosition.y + jitterAmount, originalPosition.z);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition; // reset player position after effect 
    }
}