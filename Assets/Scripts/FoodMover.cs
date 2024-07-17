using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodMover : MonoBehaviour {
    private float speed;
    private FoodSpawner.MovementPattern movementPattern;
    private Vector2 direction;
    private float waveFrequency = 3f;
    private float minY;
    private float maxY;
    private bool initialized = false;

    public void SetProperties(float speed, FoodSpawner.MovementPattern movementPattern, float minY = 0f, float maxY = 0f) {
        this.speed = speed;
        this.movementPattern = movementPattern;
        this.minY = minY;
        this.maxY = maxY;
        
        // Initialize direction for bounce movement
        if (movementPattern == FoodSpawner.MovementPattern.Bounce)
        {
            float angle = 45f * Mathf.Deg2Rad;
            if (Random.value > 0.5f) {
                direction = new Vector2(-Mathf.Cos(angle), Mathf.Sin(angle)); // Moving up-right
            }
            else {
                direction = new Vector2(-Mathf.Cos(angle), -Mathf.Sin(angle)); // Moving down-right
            }
        }
        else {
            direction = Vector2.left;
        }

        // Adjust direction based on the object's rotation
        direction = Quaternion.Euler(0, 0, transform.eulerAngles.z) * direction;
        direction.x = -Mathf.Abs(direction.x); // Ensure the x direction is leftward

        initialized = true;
    }

    private void Update() {
        if (!initialized) return;

        switch (movementPattern)  {
            case FoodSpawner.MovementPattern.Straight:
                transform.Translate(direction * speed * Time.deltaTime);
                break;
            case FoodSpawner.MovementPattern.Bounce:
                BounceMovement();
                break;
            case FoodSpawner.MovementPattern.Wave:
                WaveMovement();
                break;
        }
    }

    private void BounceMovement() {
        transform.Translate(direction * speed * Time.deltaTime);

        Vector2 newPos = transform.position;
        if (newPos.y > maxY || newPos.y < minY) {
            direction.y = -direction.y;
        }
    }

    private void WaveMovement() {
        Vector2 newPos = transform.position;
        newPos.x += direction.x * speed * Time.deltaTime;
        newPos.y = Mathf.Lerp(minY, maxY, (Mathf.Sin(Time.time * waveFrequency) + 1.0f) / 2.0f);
        transform.position = newPos;
    }
}