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

        public void IncreaseFoodSpeed(float multiplier) {
        foodSpeedMultiplier *= multiplier;

        // Increase speed of existing food instances
        FoodMover[] existingFoodMovers = FindObjectsOfType<FoodMover>();
        foreach (FoodMover foodMover in existingFoodMovers) {
            foodMover.IncreaseSpeed(multiplier);
        }
    }

    private void Start() {
        foodRangeCollider = foodRange.GetComponent<BoxCollider2D>();
        playerController = FindObjectOfType<PlayerController>();

        foreach (var food in foods) {
            var coroutine = SpawnFood(food);
            spawnCoroutines.Add(coroutine);
            StartCoroutine(coroutine);
        }
    }

    private IEnumerator SpawnFood(FoodProperties foodProperties) {
        while (true) {
            yield return new WaitForSeconds(foodProperties.spawnInterval);
            Vector2 spawnPosition = GetRandomSpawnPosition();
            Quaternion spawnRotation = foodProperties.prefab.transform.rotation; // use the prefab's default rotation

            GameObject foodInstance = Instantiate(foodProperties.prefab, spawnPosition, spawnRotation);
            FoodMover foodMover = foodInstance.AddComponent<FoodMover>();
            Food food = foodInstance.GetComponent<Food>();
            food.powerUpType = foodProperties.powerUpType; // set the power-up type

            foodMover.SetProperties(foodProperties.speed * foodSpeedMultiplier, foodProperties.movementPattern, foodRangeCollider.bounds.min.y, foodRangeCollider.bounds.max.y);

            // Check if magnet effect is active
            if (playerController.IsMagnetEffectActive()) {
                foodMover.SetMagnetTarget(playerController.transform);
            }
        }
    }

    private Vector2 GetRandomSpawnPosition() {
        float x = foodRangeCollider.bounds.center.x;
        float minY = foodRangeCollider.bounds.min.y;
        float maxY = foodRangeCollider.bounds.max.y;
        float y = Random.Range(minY, maxY);
        return new Vector2(x, y);
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