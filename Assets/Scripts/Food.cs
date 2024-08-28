using UnityEngine;
using System;

public class Food : MonoBehaviour {
    public int basePoints = 100;
    public enum PowerUpType { None, Speed, Slow, Magnet, Rotten }
    public PowerUpType powerUpType = PowerUpType.None;

    public event Action OnEaten;

    public void OnEatenTrigger(ScoreManager scoreManager, PlayerController player) {
        scoreManager.AddScore(basePoints);
        if (player != null) {
            if (powerUpType == PowerUpType.Rotten) {
                player.ActivateRottenPower();
            } else if (powerUpType != PowerUpType.None) {
                player.StorePowerUp(powerUpType);
            }
        }
        SoundManager.instance.PlayBite(); // play bite sound
        OnEaten?.Invoke();
    }
}
