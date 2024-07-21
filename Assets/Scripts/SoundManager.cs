using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance = null;

    public AudioSource biteSource;
    public AudioSource speedSource;
    public AudioSource slowSource;
    public AudioSource gameOverSource;
    public AudioSource backgroundMusicSource; // Background music source

    void Awake() {
        // Make sure there is only one instance of the SoundManager
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        // Set SoundManager to DontDestroyOnLoad so that it persists across scenes
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
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

    public void PlayGameOver() {
        gameOverSource.Play();
    }

    public void PlayBackgroundMusic() {
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.loop = true; // loop
            backgroundMusicSource.Play();
        }
    }

    public void StopBackgroundMusic() {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }
    }

    public void SetBackgroundMusicPitch(float pitch) {
        if (backgroundMusicSource != null) {
            backgroundMusicSource.pitch = pitch;
        }
    }
}
