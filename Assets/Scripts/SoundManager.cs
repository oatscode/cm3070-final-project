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
    }

    void Start() {
        PlayBackgroundMusic();
    }

    public void PlayBite() {
        PlaySound(biteSound);
    }

    public void PlaySick() {
        PlaySound(sickSound);
    }

    public void PlayLevelUp() {
        PlaySound(levelUpSound);
    }

    public void PlaySpeed() {
        PlaySound(speedSound);
    }

    public void PlayMagnet() {
        StartCoroutine(PlayMagnetLoop(magnetLoopCount));
    }

    public void PlayGameOver() {
        PlaySound(gameOverSound);
    }

    private IEnumerator PlayMagnetLoop(int loopCount) {
        for (int i = 0; i < loopCount; i++) {
            magnetSound.Play();  
            yield return new WaitForSeconds(magnetSound.clip.length);
        }
    }

    private void PlaySound(AudioSource audioSource) {
        if (audioSource != null && !audioSource.isPlaying) {
            audioSource.Play();
        }
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
        if (backgroundMusic != null) {
            backgroundMusic.pitch = pitch;
        }
    }
}