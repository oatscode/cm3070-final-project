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

    private void Start() {
        foodRangeCollider = foodRange.GetComponent<BoxCollider2D>();

        foreach (var food in foods) {
            StartCoroutine(SpawnFood(food));
        }
    }

    private IEnumerator SpawnFood(FoodProperties foodProperties) {
        while (true) {
            yield return new WaitForSeconds(foodProperties.spawnInterval);
            Vector2 spawnPosition = GetRandomSpawnPosition();
            Quaternion spawnRotation = foodProperties.prefab.transform.rotation; // Use the prefab's default rotation

            GameObject foodInstance = Instantiate(foodProperties.prefab, spawnPosition, spawnRotation);
            FoodMover foodMover = foodInstance.AddComponent<FoodMover>();
            Food food = foodInstance.GetComponent<Food>();
            if (food != null) {
                food.powerUpType = foodProperties.powerUpType; // Set the power-up type
            }
            foodMover.SetProperties(foodProperties.speed, foodProperties.movementPattern, foodRangeCollider.bounds.min.y, foodRangeCollider.bounds.max.y);
        }
    }

    private Vector2 GetRandomSpawnPosition() {
        float x = foodRangeCollider.bounds.center.x;
        float minY = foodRangeCollider.bounds.min.y;
        float maxY = foodRangeCollider.bounds.max.y;
        float y = Random.Range(minY, maxY);
        return new Vector2(x, y);
    }
}