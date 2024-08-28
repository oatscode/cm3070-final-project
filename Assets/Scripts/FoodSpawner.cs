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

    // object pool
    private Dictionary<string, Queue<GameObject>> foodPool = new Dictionary<string, Queue<GameObject>>();
    public int poolSize = 10;

    private void Start() {
        playerController = FindObjectOfType<PlayerController>();
        foodRangeCollider = foodRange.GetComponent<BoxCollider2D>();
        foodSpawnXPos = foodRangeCollider.bounds.center.x;
        foodSpawnMinYPos = foodRangeCollider.bounds.min.y;
        foodSpawnMaxYPos = foodRangeCollider.bounds.max.y;

        InitializePool();

        foreach (var food in foods) {
            StartCoroutine(SpawnFood(food));
        }
    }

    private void InitializePool() {
        foreach (var food in foods) {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++) {
                GameObject obj = Instantiate(food.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            foodPool.Add(food.prefab.name, objectPool);
        }
    }

    private GameObject GetPooledObject(string prefabName) {
        if (foodPool.ContainsKey(prefabName) && foodPool[prefabName].Count > 0) {
            GameObject obj = foodPool[prefabName].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    private void ReturnToPool(GameObject obj) {
        obj.SetActive(false);
        foodPool[obj.name].Enqueue(obj);
    }

    private IEnumerator SpawnFood(FoodProperties foodProperties) {
        while (true) {
            yield return new WaitForSeconds(foodProperties.spawnInterval);
            SpawnSingleFood(foodProperties);
        }
    }

    private void SpawnSingleFood(FoodProperties foodProperties) {
        Vector2 spawnPosition = GetRandomSpawnPosition();
        GameObject foodInstance = GetPooledObject(foodProperties.prefab.name);

        if (foodInstance == null) {
            foodInstance = Instantiate(foodProperties.prefab, spawnPosition, Quaternion.identity);
        }

        foodInstance.transform.position = spawnPosition;

        FoodMover foodMover = foodInstance.GetComponent<FoodMover>();
        Food food = foodInstance.GetComponent<Food>();
        food.powerUpType = foodProperties.powerUpType;
        
        food.OnEaten -= () => ReturnToPool(foodInstance); // detach previous listeners to avoid dupes
        food.OnEaten += () => ReturnToPool(foodInstance); // attach  listener to return the food instance to the pool

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

    public void IncreaseFoodSpeed(float multiplier) {
        foodSpeedMultiplier *= multiplier;
        foreach (FoodMover foodMover in FindObjectsOfType<FoodMover>()) {
            foodMover.IncreaseSpeed(multiplier);
        }
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