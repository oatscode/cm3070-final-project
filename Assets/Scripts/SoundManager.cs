using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance;

    public AudioSource biteSound;
    public AudioSource sickSound;
    public AudioSource speedSound;
    public AudioSource magnetSound;
    public AudioSource levelUpSound;
    public AudioSource gameOverSound;
    public AudioSource backgroundMusic;
    private int magnetLoopCount = 4;

    void Awake() {
        // ensure only one instance of SoundManager
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (instance != this) {
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject); // set SoundManager so that it persists across scenes 
    }

    void Start() {
        PlayBackgroundMusic();
    }

    public void PlayBite() {
        biteSound.Play();
    }

    public void PlaySick() {
        sickSound.Play();
    }

    public void PlayLevelUp() {
        levelUpSound.Play();
    }

    public void PlaySpeed() {
        speedSound.Play();
    }

    public void PlayMagnet() {
        StartCoroutine(PlayMagnetLoop(magnetLoopCount));
    }

    private IEnumerator PlayMagnetLoop(int loopCount) {
    for (int i = 0; i < loopCount; i++) {
        magnetSound.Play();  
        yield return new WaitForSeconds(magnetSound.clip.length);
    }
}

    public void PlayGameOver() {
        gameOverSound.Play();
    }

    public void PlayBackgroundMusic() {
        if (backgroundMusic != null && !backgroundMusic.isPlaying) {
            backgroundMusic.loop = true; // loop music
            backgroundMusic.Play();
        }
    }

    public void StopBackgroundMusic() {
        if (backgroundMusic != null && backgroundMusic.isPlaying) {
            backgroundMusic.Stop();
        }
    }

    public void SetBackgroundMusicPitch(float pitch) {
        backgroundMusic.pitch = pitch;
    }
}
