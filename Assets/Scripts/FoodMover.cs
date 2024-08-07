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
    private float waveOffset = 1.0f;  
    private float waveScale = 2.0f;

    private Transform magnetTarget;
    private FoodSpawner.MovementPattern originalMovementPattern;

    public void SetProperties(float speed, FoodSpawner.MovementPattern movementPattern, float minY = 0f, float maxY = 0f) {
        this.speed = speed;
        this.originalSpeed = speed;
        this.movementPattern = movementPattern;
        this.originalMovementPattern = movementPattern;
        this.minY = minY;
        this.maxY = maxY;

        // check if Ice Cream effect is active
        if (FindObjectOfType<PlayerController>().IsIceCreamEffectActive()) {
            this.speed /= 2; // apply slow effect
        }
        
        // initialize direction for bounce movement
        if (movementPattern == FoodSpawner.MovementPattern.Bounce) {
            float angle = 45f * Mathf.Deg2Rad;
            if (Random.value > 0.5f) {
                direction = new Vector2(-Mathf.Cos(angle), Mathf.Sin(angle)); // moving up-right
            } else {
                direction = new Vector2(-Mathf.Cos(angle), -Mathf.Sin(angle)); // moving down-right
            }
        } else {
            direction = Vector2.left;
        }

        // adjust direction based on the object's rotation
        direction = Quaternion.Euler(0, 0, transform.eulerAngles.z) * direction;
        direction.x = -Mathf.Abs(direction.x); // ensure the x direction is leftward

        initialized = true;
    }

    public void IncreaseSpeed(float multiplier) {
        speed *= multiplier;
    }

    private void Update() {
        if (!initialized) return;

        // check if Ice Cream effect is active
        if (FindObjectOfType<PlayerController>().IsIceCreamEffectActive()) {
            speed = originalSpeed / 2; // apply slow effect
        } else {
            speed = originalSpeed;
        }

        if (magnetTarget != null) {
            MoveTowardsMagnetTarget();
        } else {
            switch (movementPattern) {
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
        newPos.y = Mathf.Lerp(minY, maxY, (Mathf.Sin(Time.time * waveFrequency * speed / originalSpeed) + waveOffset) / waveScale); // adjust wave frequency based on speed
        transform.position = newPos;

        // calculate the direction vector for the rotation
        Vector2 movementDirection = new Vector2(direction.x * speed, Mathf.Cos(Time.time * waveFrequency * speed / originalSpeed) * (maxY - minY) / 2.0f);
        float angle = (Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg) + 180;

        // rotate the fish to point in the direction of movement
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

    private void MoveTowardsMagnetTarget() {
        Vector2 directionToTarget = (magnetTarget.position - transform.position).normalized;
        transform.Translate(directionToTarget * speed * Time.deltaTime);
    }

    public void SetMagnetTarget(Transform target) {
        magnetTarget = target;
    }

    public void ResetToOriginalMovementPattern() {
        magnetTarget = null;
        movementPattern = originalMovementPattern;
    }
}