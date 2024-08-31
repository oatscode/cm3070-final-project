using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance; // static to ensure only a single instance exists

    // public variables for sound effects and background music
    public AudioSource biteSound;
    public AudioSource sickSound;
    public AudioSource speedSound;
    public AudioSource magnetSound;
    public AudioSource levelUpSound;
    public AudioSource gameOverSound;
    public AudioSource backgroundMusic;

    private int magnetLoopCount = 4; // number of times the magnet sound effect loops

    // ensure only one SoundManager instance exists
    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // persist across scenes
        } else if (instance != this) {
            Destroy(gameObject);
        }
    }

    // starts background music when the game begins
    void Start() {
        PlayBackgroundMusic(); 
    }

    // plays the "bite" sound effect when the player eats food
    public void PlayBite() {
        PlaySound(biteSound);
    }

    // plays the "sick" sound effect when the player eats rotten food
    public void PlaySick() {
        PlaySound(sickSound);
    }

    // plays the "level up" sound effect when the player levels up
    public void PlayLevelUp() {
        PlaySound(levelUpSound);
    }

    // plays the "speed up" sound effect when the player activates the speed power-up
    public void PlaySpeed() {
        PlaySound(speedSound);
    }

    // plays the "pull" sound effect when the player activates the magnet power-up
    public void PlayMagnet() {
        StartCoroutine(PlayMagnetLoop(magnetLoopCount)); // loop it a few times to sound like a tractor beam
    }

    // plays the "game over" sound effect
    public void PlayGameOver() {
        PlaySound(gameOverSound);
    }
    // loop the magnet sound effect a number of times
    private IEnumerator PlayMagnetLoop(int loopCount) {
        for (int i = 0; i < loopCount; i++) {
            magnetSound.Play();  
            yield return new WaitForSeconds(magnetSound.clip.length); // let clip finish before replaying
        }
    }

    // plays a passed sound effect if it is not already playing
    private void PlaySound(AudioSource audioSource) {
        if (audioSource != null && !audioSource.isPlaying) {
            audioSource.Play();
        }
    }

    // plays the background music in a loop
    public void PlayBackgroundMusic() {
        if (backgroundMusic != null && !backgroundMusic.isPlaying) {
            backgroundMusic.loop = true; // loop music
            backgroundMusic.Play();
        }
    }

    // stops the background music if it is playing
    public void StopBackgroundMusic() {
        if (backgroundMusic != null && backgroundMusic.isPlaying) {
            backgroundMusic.Stop();
        }
    }

    // adjusts the pitch of the background music
    public void SetBackgroundMusicPitch(float pitch) {
        if (backgroundMusic != null) {
            backgroundMusic.pitch = pitch;
        }
    }
}