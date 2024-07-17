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
            switch (powerUpType) {
                case PowerUpType.Chilli:
                    player.ActivateSpeedBoost();
                    break;
                case PowerUpType.IceCream:
                    //player.ActivateIceCreamEffect();
                    break;
                case PowerUpType.Magnet:
                    //player.ActivateMagnetEffect();
                    break;
            }
        }
        Destroy(gameObject);
    }
}
