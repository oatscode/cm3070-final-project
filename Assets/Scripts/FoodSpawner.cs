using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour {
    // class to store properties and spawn each type of food
    [System.Serializable]
    public class FoodProperties {
        public GameObject prefab; // prefab of the food to be spawned
        public float spawnInterval; // time interval between spawns of this food type
        public float speed;
        public MovementPattern movementPattern;
        public Food.PowerUpType powerUpType;
    }

    // the different movement patterns a food can have
    public enum MovementPattern {
        Straight,
        Bounce,
        Wave
    }

    public List<FoodProperties> foods; // list of food types and their properties
    public GameObject foodRange; // defines the vertical range within which food can spawn
    public int poolSize = 10; // size of the object pool for each food type

    private BoxCollider2D foodRangeCollider; // collider to define food spawn range
    private List<IEnumerator> spawnCoroutines = new List<IEnumerator>();
    private PlayerController playerController; // to access player data
    private float foodSpeedMultiplier = 1.0f; // multiplier to adjust the speed of the food
    private float foodSpawnXPos; // x position where food spawns
    private float foodSpawnMinYPos; // minimum Y position for food spawn
    private float foodSpawnMaxYPos; // maximum Y position for food spawn

    // dictionary to store pooled food objects for each food type
    private Dictionary<string, Queue<GameObject>> foodPool = new Dictionary<string, Queue<GameObject>>();

    private void Start() {
        // initialise references and variables
        playerController = FindObjectOfType<PlayerController>();
        foodRangeCollider = foodRange.GetComponent<BoxCollider2D>();
        foodSpawnXPos = foodRangeCollider.bounds.center.x;
        foodSpawnMinYPos = foodRangeCollider.bounds.min.y;
        foodSpawnMaxYPos = foodRangeCollider.bounds.max.y;

        InitialisePool(); // initialise the object pool

        // start spawning food items based on their properties
        foreach (var food in foods) {
            StartCoroutine(SpawnFood(food));
        }
    }

    // initialises the object pool for each food type
    private void InitialisePool() {
        foreach (var food in foods) {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // instantiate a number of food objects up to the pool size and deactivate them
            for (int i = 0; i < poolSize; i++) {
                GameObject obj = Instantiate(food.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            foodPool.Add(food.prefab.name, objectPool); // add the pool to the dictionary
        }
    }

    // retrieves a pooled object from the pool if available
    private GameObject GetPooledObject(string prefabName) {
        if (foodPool.ContainsKey(prefabName) && foodPool[prefabName].Count > 0) {
            GameObject obj = foodPool[prefabName].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null; // return null if no object is available in the pool
    }

    // returns a food object to the pool after it has been used
    private void ReturnToPool(GameObject obj) {
        obj.SetActive(false);
        foodPool[obj.name].Enqueue(obj);
    }

    // spawn food items at regular intervals
    private IEnumerator SpawnFood(FoodProperties foodProperties) {
        while (true) {
            yield return new WaitForSeconds(foodProperties.spawnInterval); // wait for the next spawn interval
            SpawnSingleFood(foodProperties);
        }
    }

    // spawn a single food item based on its properties
    private void SpawnSingleFood(FoodProperties foodProperties) {
        Vector2 spawnPosition = GetRandomSpawnPosition(); // get a random position within the spawn range
        GameObject foodInstance = GetPooledObject(foodProperties.prefab.name); // get a pooled food

        // if no pooled food is available, instantiate a new one
        if (foodInstance == null) {
            foodInstance = Instantiate(foodProperties.prefab, spawnPosition, Quaternion.identity);
        }

        foodInstance.transform.position = spawnPosition;

        // set the properties of the food mover component (responsible for movement)
        FoodMover foodMover = foodInstance.GetComponent<FoodMover>();
        Food food = foodInstance.GetComponent<Food>();
        food.powerUpType = foodProperties.powerUpType; // assign the power-up
        
        // ensure no duplicate listeners by removing the previous one and adding a new one
        food.OnEaten -= () => ReturnToPool(foodInstance);
        food.OnEaten += () => ReturnToPool(foodInstance);

        foodMover.SetProperties(foodProperties.speed * foodSpeedMultiplier,
            foodProperties.movementPattern, foodSpawnMinYPos, foodSpawnMaxYPos);
        
        // if the magnet power-up is active, make the food move towards the player
        if (playerController.IsMagnetEffectActive()) {
            foodMover.SetMagnetTarget(playerController.transform);
        }
    }

    // returns a random Y position within the spawn range for the food
    private Vector2 GetRandomSpawnPosition() {
        float foodSpawnYPos = Random.Range(foodSpawnMinYPos, foodSpawnMaxYPos);
        return new Vector2(foodSpawnXPos, foodSpawnYPos);
    }

    // increase the speed of all food items
    public void IncreaseFoodSpeed(float multiplier) {
        foodSpeedMultiplier *= multiplier; // increase the multiplier
        foreach (FoodMover foodMover in FindObjectsOfType<FoodMover>()) {
            foodMover.IncreaseSpeed(multiplier); // increase the speed of each active food mover
        }
    }
}