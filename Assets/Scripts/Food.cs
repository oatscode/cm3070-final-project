using UnityEngine;
using System;

public class Food : MonoBehaviour {
    // public variables adjustable in Unity
    public int basePoints = 100; // base points awarded when this food is eaten
    public enum PowerUpType { None, Speed, Slow, Magnet, Rotten } // potential power-ups applicable to this food
    public PowerUpType powerUpType = PowerUpType.None; // foods have no power up by default

    // an event triggered when the food is eaten
    public event Action OnEaten;

    // logic when this food is eaten
    public void OnEatenTrigger(ScoreManager scoreManager, PlayerController player) {
        scoreManager.AddScore(basePoints); // add the base points to the score

        // if the food is "rotten", apply the rotten effect to the player
        // otherwise, if the food is a power-up, store it for later use by the player
        if (powerUpType == PowerUpType.Rotten) {
            player.ActivateRottenPower();
        } else if (powerUpType != PowerUpType.None) {
            player.StorePowerUp(powerUpType);
        }
    
        SoundManager.instance.PlayBite(); // play sound effect for eating food
        OnEaten?.Invoke(); // event trigger
    }
}
