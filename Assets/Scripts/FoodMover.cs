using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodMover : MonoBehaviour {
    private float speed;
    private float originalSpeed;
    private FoodSpawner.MovementPattern movementPattern;
    private Vector2 direction;
    private float waveFrequency = 3f;
    private float minY;
    private float maxY;
    private bool initialized = false;
    private float waveOffset = 1.0f;  // Offset to shift the sine wave from [-1, 1] to [0, 2]
    private float waveScale = 2.0f;   // Scale to normalize the sine wave to [0, 1]

    public void SetProperties(float speed, FoodSpawner.MovementPattern movementPattern, float minY = 0f, float maxY = 0f) {
        this.speed = speed;
        this.originalSpeed = speed;
        this.movementPattern = movementPattern;
        this.minY = minY;
        this.maxY = maxY;

        // Check if Ice Cream effect is active
        if (FindObjectOfType<PlayerController>().IsIceCreamEffectActive()) {
            this.speed /= 2; // Apply slow effect
        }
        
        // Initialize direction for bounce movement
        if (movementPattern == FoodSpawner.MovementPattern.Bounce) {
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
        newPos.y = Mathf.Lerp(minY, maxY, (Mathf.Sin(Time.time * waveFrequency * speed / originalSpeed) + waveOffset) / waveScale); // Adjust wave frequency based on speed
        transform.position = newPos;

        // Calculate the direction vector for the rotation
        Vector2 movementDirection = new Vector2(direction.x * speed, Mathf.Cos(Time.time * waveFrequency * speed / originalSpeed) * (maxY - minY) / 2.0f);
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        // Adjust the angle by 180 degrees to correct the facing direction
         angle += 180;
        // Rotate the fish to point in the direction of movement
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    public void SetSpeed(float newSpeed) {
        speed = newSpeed;
    }

    public float GetSpeed() {
        return speed;
    }

    public float GetOriginalSpeed() {
        return originalSpeed;
    }

    public void ResetToOriginalSpeed() {
        speed = originalSpeed;
    }
}