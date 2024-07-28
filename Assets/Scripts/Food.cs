using UnityEngine;

public class Food : MonoBehaviour {
    public int points = 100;
    public enum PowerUpType { None, Chilli, IceCream, Magnet }
    public PowerUpType powerUpType = PowerUpType.None;

    public void OnEaten(ScoreManager scoreManager, PlayerController player) {
        if (scoreManager != null) {
            scoreManager.AddScore(points);
        }
        if (player != null) {
            Debug.Log(powerUpType);
            switch (powerUpType) {
                case PowerUpType.Chilli:
                    //SoundManager.instance.PlaySpeed();
                    player.StorePowerUp(powerUpType);
                    break;
                case PowerUpType.IceCream:
                    //SoundManager.instance.PlaySlow();
                    player.StorePowerUp(powerUpType);
                    break;
                case PowerUpType.Magnet:
                    player.StorePowerUp(powerUpType);
                    break;
            }
        }
        SoundManager.instance.PlayBite();
        Destroy(gameObject);
    }
}
