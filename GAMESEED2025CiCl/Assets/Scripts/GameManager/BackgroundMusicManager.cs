using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundMusicManager : MonoBehaviour
{
    public List<AudioClip> backgroundMusicClips; // List of background music clips
    public AudioSource audioSource; // Reference to the AudioSource component

    void Start()
    {
        // Ensure an AudioSource component is present
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Start playing background music
        StartCoroutine(PlayBackgroundMusic());
    }

    IEnumerator PlayBackgroundMusic()
    {
        while (true)
        {
            if (backgroundMusicClips.Count > 0)
            {
                // Play a random music clip
                int randomIndex = Random.Range(0, backgroundMusicClips.Count);
                audioSource.clip = backgroundMusicClips[randomIndex];
                audioSource.Play();

                // Wait until the current clip finishes playing
                yield return new WaitForSeconds(audioSource.clip.length);
            }
            else
            {
                Debug.LogWarning("No background music clips assigned to BackgroundMusicManager.");
                yield return null; // Wait for a frame before checking again
            }
        }
    }
}
