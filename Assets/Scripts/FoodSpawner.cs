using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FoodProperties
    {
        public GameObject prefab;
        public float spawnInterval;
        public float speed;
        public MovementPattern movementPattern;
    }

    public enum MovementPattern
    {
        Straight,
        Bounce,
        Wave
    }

    public List<FoodProperties> foods;

    public GameObject foodRange;
    private BoxCollider2D foodRangeCollider;

    private void Start()
    {
        foodRangeCollider = foodRange.GetComponent<BoxCollider2D>();

        foreach (var food in foods)
        {
            StartCoroutine(SpawnFood(food));
        }
    }

    private IEnumerator SpawnFood(FoodProperties foodProperties)
    {
        while (true)
        {
            yield return new WaitForSeconds(foodProperties.spawnInterval);
            Vector2 spawnPosition = GetRandomSpawnPosition();
            GameObject foodInstance = Instantiate(foodProperties.prefab, spawnPosition, Quaternion.identity);
            FoodMover foodMover = foodInstance.AddComponent<FoodMover>();
            // if (foodProperties.movementPattern == MovementPattern.Wave)
            // {
                foodMover.SetProperties(foodProperties.speed, foodProperties.movementPattern, foodRangeCollider.bounds.min.y, foodRangeCollider.bounds.max.y);
            // }
            // else
            // {
                // foodMover.SetProperties(foodProperties.speed, foodProperties.movementPattern);
            // }
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float x = foodRangeCollider.bounds.center.x;
        float minY = foodRangeCollider.bounds.min.y;
        float maxY = foodRangeCollider.bounds.max.y;
        float y = Random.Range(minY, maxY);
        return new Vector2(x, y);
    }
}