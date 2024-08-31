using UnityEngine;

public class FoodMover : MonoBehaviour {
    // variables for internal logic
    private float speed; // current speed of the food item
    private float originalSpeed; // original speed of the food item
    private FoodSpawner.MovementPattern movementPattern;
    private Vector2 direction; // direction food movement
    private float waveFrequency = 3f; // frequency of the wave pattern movement
    private float minY; // minimum Y position for movement pattern boundaries
    private float maxY; // maximum Y position for movement pattern boundaries
    private bool initialised = false; // flag to check if the food mover has been initialised
    private float waveOffset = 1.0f; // offset for wave movement pattern calculation  
    private float waveScale = 2.0f; // scale for wave movement calculation

    private Transform magnetTarget; // target for food to move towards when the magnet effect is active
    private FoodSpawner.MovementPattern originalMovementPattern; // original movement pattern to reset to after magnet effect

    // set the properties of the food item: speed, movement pattern, boundaries
    public void SetProperties(float speed, FoodSpawner.MovementPattern movementPattern, float minY = 0f, float maxY = 0f) {
        this.speed = speed;
        this.originalSpeed = speed; // store the original speed
        this.movementPattern = movementPattern;
        this.originalMovementPattern = movementPattern; // store the original movement pattern
        this.minY = minY;
        this.maxY = maxY;

        // halve the food speed if the slow effect is active
        if (FindObjectOfType<PlayerController>().IsSlowEffectActive()) {
            this.speed /= 2;
        }
        
        // initialise direction for bounce movement
        if (movementPattern == FoodSpawner.MovementPattern.Bounce) {
            float angle = 45f * Mathf.Deg2Rad;
            if (Random.value > 0.5f) {
                direction = new Vector2(-Mathf.Cos(angle), Mathf.Sin(angle)); // moving up and right
            } else {
                direction = new Vector2(-Mathf.Cos(angle), -Mathf.Sin(angle)); // moving down and right
            }
        } else {
            direction = Vector2.left; // default direction is leftward for other movement patterns
        }

        // adjust direction based on the object's rotation
        direction = Quaternion.Euler(0, 0, transform.eulerAngles.z) * direction;
        direction.x = -Mathf.Abs(direction.x); // ensure the x direction is leftward

        initialised = true; // mark the food mover as initialised
    }

    // increase the speed of the food item by a multiplier
    public void IncreaseSpeed(float multiplier) {
        speed *= multiplier;
    }

    // update the food item's position and handle different movement patterns
    private void Update() {
        if (!initialised) return;

        // check if the slow effect is active and adjust speed if so
        if (FindObjectOfType<PlayerController>().IsSlowEffectActive()) {
            speed = originalSpeed / 2;
        } else {
            speed = originalSpeed; // reset to original speed if slow effect is not active
        }

        // if the magnet effect is active:
        //  move all food items towards the player
        //  else, apply the appropriate movement pattern
        if (magnetTarget != null) {
            MoveTowardsMagnetTarget();
        } else {
            switch (movementPattern) {
                case FoodSpawner.MovementPattern.Straight:
                    transform.Translate(direction * speed * Time.deltaTime, Space.World);
                    break;
                case FoodSpawner.MovementPattern.Bounce:
                    BounceMovement();
                    break;
                case FoodSpawner.MovementPattern.Wave:
                    WaveMovement();
                    break;
            }
        }
    }

    // bounce movement pattern
    private void BounceMovement() {
        transform.Translate(direction * speed * Time.deltaTime); // move in the preset direction

        // reverse the Y direction if the food item hits the top or bottom boundary
        Vector2 newPos = transform.position;
        if (newPos.y >= maxY || newPos.y <= minY) {
            direction.y = -direction.y;
        }
    }

    // wave pattern movement
    private void WaveMovement() {
        Vector2 newPos = transform.position;
        newPos.x += direction.x * speed * Time.deltaTime;
        // calculate Y position using a sine wave function to create wave motion
        newPos.y = Mathf.Lerp(minY, maxY, (Mathf.Sin(Time.time * waveFrequency * speed / originalSpeed) + waveOffset) / waveScale); // adjust wave frequency based on speed
        transform.position = newPos;

        // calculate the direction vector for the rotation
        Vector2 movementDirection = new Vector2(direction.x * speed, Mathf.Cos(Time.time * waveFrequency * speed / originalSpeed) * (maxY - minY) / 2.0f);
        float angle = (Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg) + 180;

        // rotate the food item to point in the direction of movement
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    // set a new speed for the food item
    public void SetSpeed(float newSpeed) {
        speed = newSpeed;
    }

    // get the current speed of the food item
    public float GetSpeed() {
        return speed;
    }

    // get the original speed of the food item
    public float GetOriginalSpeed() {
        return originalSpeed;
    }

    // reset the food item to its original speed
    public void ResetToOriginalSpeed() {
        speed = originalSpeed;
    }

    // move the food item towards a target (player) when the magnet effect is active
    private void MoveTowardsMagnetTarget() {
        Vector2 directionToTarget = (magnetTarget.position - transform.position).normalized;
        transform.Translate(directionToTarget * speed * Time.deltaTime, Space.World);
    }

    // set the magnet target
    public void SetMagnetTarget(Transform target) {
        magnetTarget = target;
    }
    
    // reset the food item's movement pattern to its original state
    public void ResetToOriginalMovementPattern() {
        magnetTarget = null;
        movementPattern = originalMovementPattern;
    }
}