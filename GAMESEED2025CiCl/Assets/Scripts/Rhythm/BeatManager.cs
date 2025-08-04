using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections; // Ensure this is present and correct

public class BeatManager : MonoBehaviour
{
    public PlayerStats playerStats; // Reference to the PlayerStats script
    public AudioSource audioSource; // AudioSource for the cling sound
    public AudioClip clingSound; // The sound for a perfect hit
    public TextMeshProUGUI feedbackText; // UI Text to display feedback

    public float bpm = 130f;
    private float secPerBeat; // Seconds per beat
    private float nextBeatTime; // The time when the next beat is expected

    private const float PERFECT_WINDOW = 0.15f; // +/- 0.15 seconds for perfect
    private const float OKAY_WINDOW = 0.3f;    // +/- 0.3 seconds for okay

    void Start()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("PlayerStats not found in the scene. Please assign it or ensure it exists.");
                enabled = false; // Disable script if PlayerStats is missing
                return;
            }
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (feedbackText == null)
        {
            Debug.LogWarning("Feedback Text (TextMeshProUGUI) not assigned. Beat feedback will not be displayed.");
        }

        secPerBeat = 60f / bpm;
        nextBeatTime = playerStats.timePlayedThisRun + secPerBeat; // Initialize next beat time
    }

    void Update()
    {
        // Check if it's time for the next beat
        if (playerStats.timePlayedThisRun >= nextBeatTime)
        {
            // If the player missed the previous beat, advance to the next one
            nextBeatTime += secPerBeat;
        }

        // Check for player input (e.g., a specific key press for rhythm action)
        if (Input.GetKeyDown(KeyCode.Space)) // Example: Player presses Spacebar
        {
            CheckBeatAccuracy();
        }
    }

    void CheckBeatAccuracy()
    {
        float timeSinceLastBeat = playerStats.timePlayedThisRun - (nextBeatTime - secPerBeat);
        float timeToNextBeat = nextBeatTime - playerStats.timePlayedThisRun;

        // Calculate the absolute difference from the nearest beat center
        float beatDifference = Mathf.Min(Mathf.Abs(timeSinceLastBeat), Mathf.Abs(timeToNextBeat));

        string feedback = "";
        Color feedbackColor = Color.white;

        if (beatDifference <= PERFECT_WINDOW)
        {
            feedback = "PERFECT!";
            feedbackColor = Color.green;
            if (clingSound != null) audioSource.PlayOneShot(clingSound);
        }
        else if (beatDifference <= OKAY_WINDOW)
        {
            feedback = "OKAY!";
            feedbackColor = Color.yellow;
        }
        else
        {
            feedback = "BAD!";
            feedbackColor = Color.red;
        }

        DisplayFeedback(feedback, feedbackColor);

        // Advance to the next beat after a hit, regardless of accuracy
        // This prevents multiple hits on a single beat window
        nextBeatTime = playerStats.timePlayedThisRun + secPerBeat;
    }

    void DisplayFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            StopAllCoroutines(); // Stop any previous fade out
            StartCoroutine(FadeOutFeedbackText());
        }
    }

    System.Collections.IEnumerator FadeOutFeedbackText()
    {
        yield return new WaitForSeconds(0.5f); // Display for 0.5 seconds
        float fadeDuration = 0.5f; // Fade out over 0.5 seconds
        Color startColor = feedbackText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        float timer = 0;
        while (timer < fadeDuration)
        {
            feedbackText.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        feedbackText.text = ""; // Clear text after fading
    }
}