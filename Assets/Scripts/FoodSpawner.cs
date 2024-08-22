using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour {
    [System.Serializable]
    public class FoodProperties {
        public GameObject prefab;
        public float spawnInterval;
        public float speed;
        public MovementPattern movementPattern;
        public Food.PowerUpType powerUpType;
    }

    public enum MovementPattern {
        Straight,
        Bounce,
        Wave
    }

    public List<FoodProperties> foods;
    public GameObject foodRange;
    private BoxCollider2D foodRangeCollider;
    private List<IEnumerator> spawnCoroutines = new List<IEnumerator>();
    private PlayerController playerController;
    private float foodSpeedMultiplier = 1.0f;
    private float foodSpawnXPos;
    private float foodSpawnMinYPos;
    private float foodSpawnMaxYPos;

    public void IncreaseFoodSpeed(float multiplier) {
        foodSpeedMultiplier *= multiplier;
        foreach (FoodMover foodMover in FindObjectsOfType<FoodMover>()) {
            foodMover.IncreaseSpeed(multiplier);
        }
    }

    private void Start() {
        playerController = FindObjectOfType<PlayerController>();
        foodRangeCollider = foodRange.GetComponent<BoxCollider2D>();
        foodSpawnXPos = foodRangeCollider.bounds.center.x;
        foodSpawnMinYPos = foodRangeCollider.bounds.min.y;
        foodSpawnMaxYPos = foodRangeCollider.bounds.max.y;

        foreach (var food in foods) {
            StartCoroutine(SpawnFood(food));
        }
    }

    private IEnumerator SpawnFood(FoodProperties foodProperties) {
        while (true) {
            yield return new WaitForSeconds(foodProperties.spawnInterval);
            SpawnSingleFood(foodProperties);
        }
    }

    private void SpawnSingleFood(FoodProperties foodProperties) {
        Vector2 spawnPosition = GetRandomSpawnPosition();
        GameObject foodInstance = Instantiate(foodProperties.prefab, spawnPosition, Quaternion.identity);
        FoodMover foodMover = foodInstance.AddComponent<FoodMover>();
        Food food = foodInstance.GetComponent<Food>();
        food.powerUpType = foodProperties.powerUpType;
        foodMover.SetProperties(foodProperties.speed * foodSpeedMultiplier, 
            foodProperties.movementPattern, foodSpawnMinYPos, foodSpawnMaxYPos);
        
        if (playerController.IsMagnetEffectActive()) {
            foodMover.SetMagnetTarget(playerController.transform);
        }
    }

    private Vector2 GetRandomSpawnPosition() {
        float foodSpawnYPos = Random.Range(foodSpawnMinYPos, foodSpawnMaxYPos);
        return new Vector2(foodSpawnXPos, foodSpawnYPos);
    }

    public void AdjustSpawnIntervals(float multiplier) {
        StopAllCoroutines();
        for (int i = 0; i < foods.Count; i++) {
            foods[i].spawnInterval *= multiplier;
            var coroutine = SpawnFood(foods[i]);
            spawnCoroutines[i] = coroutine;
            StartCoroutine(coroutine);
        }
    }

    public void ResetSpawnIntervals(float originalMultiplier) {
        AdjustSpawnIntervals(originalMultiplier);
    }
}