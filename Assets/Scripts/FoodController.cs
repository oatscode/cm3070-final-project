using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : MonoBehaviour {
    public GameObject foodPrefab;
    public GameObject hotFoodPrefab;
    public float spawnInterval = 2.0f;  // Time interval between spawns
    public float hotFoodSpawnInterval = 4.0f;
    public float moveSpeed = 5.0f;
    public float hotFoodMoveSpeed = 10.0f;
    public GameObject foodRange; // Reference to the FoodRange GameObject
    private float minY;
    private float maxY;

     IEnumerator SpawnFood() {
        while (true) {
            yield return new WaitForSeconds(spawnInterval);

            float randomY = Random.Range(minY, maxY);
            Vector2 spawnPosition = new Vector2(8, randomY);

            GameObject foodInstance = Instantiate(foodPrefab, spawnPosition, Quaternion.identity, transform);
            // Debug.Log($"GreenFood spawned at position: {spawnPosition}");

            StartCoroutine(MoveFood(foodInstance, moveSpeed));
        }
     }

        IEnumerator SpawnHotFood() {
        while (true) {
            yield return new WaitForSeconds(hotFoodSpawnInterval);

            float randomY = Random.Range(minY, maxY);
            Vector2 spawnPosition = new Vector2(8, randomY);

            GameObject hotFoodInstance = Instantiate(hotFoodPrefab, spawnPosition, Quaternion.identity, transform);
            StartCoroutine(MoveFood(hotFoodInstance, hotFoodMoveSpeed));
        }
    }

    IEnumerator MoveFood(GameObject food, float speed) {   
        while (true) {
            food.transform.Translate(Vector2.left * speed * Time.deltaTime);
            yield return null;
        }
    }

    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     // Check if the colliding object has the tag "Food"
    //     if (collision.gameObject.tag == "Boundary")
    //     {
    //         // Destroy the colliding game object
    //         Destroy(collision.gameObject);
    //     }
    // }

    // Start is called before the first frame update
    void Start() {
        // Get the Box Collider 2D component of the FoodRange
        BoxCollider2D foodRangeCollider = foodRange.GetComponent<BoxCollider2D>();

        // Calculate the min and max Y values based on the FoodRange's size and position
        minY = foodRangeCollider.bounds.min.y;
        maxY = foodRangeCollider.bounds.max.y;

        StartCoroutine(SpawnFood());
        StartCoroutine(SpawnHotFood());
    }

    // Update is called once per frame
    void Update() {
        
    }
}
