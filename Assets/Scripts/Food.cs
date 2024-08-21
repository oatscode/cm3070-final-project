using UnityEngine;

public class Food : MonoBehaviour {
    public int basePoints = 100;
    public enum PowerUpType { None, Speed, Slow, Magnet, Rotten }
    public PowerUpType powerUpType = PowerUpType.None;

    public void OnEaten(ScoreManager scoreManager, PlayerController player) {
        scoreManager.AddScore(basePoints);
        if (player != null) {
            switch (powerUpType) {
                case PowerUpType.Speed:
                    player.StorePowerUp(powerUpType);
                    break;
                case PowerUpType.Slow:
                    player.StorePowerUp(powerUpType);
                    break;
                case PowerUpType.Magnet:
                    player.StorePowerUp(powerUpType);
                    break;
                case PowerUpType.Rotten:
                    player.ActivateRottenPower();
                    break;
            }
        }
        SoundManager.instance.PlayBite(); // play bite sound
        Destroy(gameObject); // destroy the food instance
    }
}
