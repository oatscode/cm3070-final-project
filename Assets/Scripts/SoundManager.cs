using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance = null;

    public AudioSource biteSource;
    public AudioSource speedSource;
    public AudioSource slowSource;
    public AudioSource magnetSource;
    public AudioSource gameOverSource;
    public AudioSource backgroundMusicSource;

    void Awake() {
        // ensure only one instance of SoundManager
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }

        // set SoundManager to DontDestroyOnLoad so that it persists across scene change
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        PlayBackgroundMusic();
    }

    public void PlayBite() {
        biteSource.Play();
    }

    public void PlaySpeed() {
        speedSource.Play();
    }

    public void PlaySlow() {
        slowSource.Play();
    }

    public void PlayMagnet() {
        magnetSource.Play();
    }

    public void PlayGameOver() {
        gameOverSource.Play();
    }

    public void PlayBackgroundMusic() {
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying) {
            backgroundMusicSource.loop = true; // loop music
            backgroundMusicSource.Play();
        }
    }

    public void StopBackgroundMusic() {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying) {
            backgroundMusicSource.Stop();
        }
    }

    public void SetBackgroundMusicPitch(float pitch) {
        backgroundMusicSource.pitch = pitch;
    }
}
