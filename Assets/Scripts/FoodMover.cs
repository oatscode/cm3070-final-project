using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodMover : MonoBehaviour
{
    private float speed;
    private FoodSpawner.MovementPattern movementPattern;
    private Vector2 direction;
    private float waveFrequency = 3f;
    private float minY;
    private float maxY;

    public void SetProperties(float speed, FoodSpawner.MovementPattern movementPattern, float minY = 0f, float maxY = 0f)
    {
        this.speed = speed;
        this.movementPattern = movementPattern;
        this.minY = minY;
        this.maxY = maxY;
        direction = Vector2.left;
    }

    private void Update()
    {
        switch (movementPattern)
        {
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

    private void BounceMovement()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        Vector2 newPos = transform.position;
        if (newPos.y > Camera.main.orthographicSize || newPos.y < -Camera.main.orthographicSize)
        {
            direction.y = -direction.y;
        }
    }

    private void WaveMovement()
    {
        Vector2 newPos = transform.position;
        newPos.x += direction.x * speed * Time.deltaTime;
        newPos.y = Mathf.Lerp(minY, maxY, (Mathf.Sin(Time.time * waveFrequency) + 1.0f) / 2.0f);
        transform.position = newPos;
    }
}